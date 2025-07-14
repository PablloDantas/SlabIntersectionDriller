using System.IO;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.Infrastructure.RevitAPI;
using Microsoft.Win32;
using InvalidOperationException = Autodesk.Revit.Exceptions.InvalidOperationException;

namespace ClashOpenings.Presentation.Commands;

[Transaction(TransactionMode.Manual)]
public class ClashDetectionCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        var repo = new RevitRepository(doc);
        var pipes = repo.GetByCategory(BuiltInCategory.OST_PipeCurves).ToList();
        var floors = repo.GetByCategory(BuiltInCategory.OST_Floors).ToList();

        var floorClashesMap = new Dictionary<ElementId, List<ElementId>>();

        foreach (var floor in floors)
        {
            var floorId = floor.Id;
            var clashResults = new List<ElementId>();

            var floorBbox = floor.get_BoundingBox(null);
            var floorOutline = new Outline(floorBbox.Min, floorBbox.Max);
            var bboxFilter = new BoundingBoxIntersectsFilter(floorOutline);

            var candidateElements = new FilteredElementCollector(doc).WherePasses(bboxFilter).ToElements();

            if (candidateElements.Count == 0) continue;

            var floorSolid = GetSolidFromElement(floor);
            if (floorSolid == null) continue;

            foreach (var candidate in candidateElements)
            {
                if (candidate.Id.Equals(floorId) || candidate is not (MEPCurve or FamilyInstance)) continue;

                var candidateSolid = GetSolidFromElement(candidate);
                if (candidateSolid == null) continue;

                try
                {
                    var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                        floorSolid, candidateSolid, BooleanOperationsType.Intersect);

                    if (intersection != null && intersection.Volume > 1e-9) clashResults.Add(candidate.Id);
                }
                catch (InvalidOperationException)
                {
                    // Lidar com falhas na operação booleana
                }
            }

            // Adicionar ao dicionário apenas se houver colisões
            if (clashResults.Count > 0) floorClashesMap[floorId] = clashResults;
        }

        // Exibir resultados para o usuário
        if (floorClashesMap.Count == 0)
        {
            TaskDialog.Show("Detecção de Conflitos", "Nenhum conflito foi encontrado entre pisos e elementos.");
            return Result.Succeeded;
        }

        // Contar o número total de conflitos
        var totalClashes = floorClashesMap.Values.Sum(list => list.Count);

        // Criar uma mensagem resumida
        var summaryBuilder = new StringBuilder();
        summaryBuilder.AppendLine($"Foram encontrados {totalClashes} conflitos em {floorClashesMap.Count} pisos.");
        summaryBuilder.AppendLine();
        summaryBuilder.AppendLine("Resumo por piso:");

        foreach (var kvp in floorClashesMap)
        {
            var floor = doc.GetElement(kvp.Key);
            var floorName = floor.Name;
            if (string.IsNullOrEmpty(floorName))
                floorName = $"ID: {kvp.Key.IntegerValue}";

            summaryBuilder.AppendLine($"- {floorName}: {kvp.Value.Count} conflitos");
        }

        // Perguntar ao usuário se deseja visualizar os conflitos
        var td = new TaskDialog("Resultado da Detecção de Conflitos")
        {
            MainInstruction = $"Foram encontrados {totalClashes} conflitos.",
            MainContent = summaryBuilder.ToString(),
            CommonButtons = TaskDialogCommonButtons.Ok,
            DefaultButton = TaskDialogResult.Ok
        };

        td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Visualizar conflitos na vista ativa");
        td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Exportar relatório detalhado");

        var result = td.Show();

        if (result == TaskDialogResult.CommandLink1)
            // Visualizar conflitos na vista ativa
            HighlightClashesInActiveView(uiDoc, floorClashesMap);
        else if (result == TaskDialogResult.CommandLink2)
            // Exportar relatório detalhado
            ExportClashReport(doc, floorClashesMap);

        return Result.Succeeded;
    }

    /// <summary>
    ///     Destaca os elementos em conflito na vista ativa
    /// </summary>
    private void HighlightClashesInActiveView(UIDocument uiDoc, Dictionary<ElementId, List<ElementId>> floorClashesMap)
    {
        // Coletar todos os IDs em conflito (pisos e elementos)
        var allClashIds = new List<ElementId>();

        foreach (var kvp in floorClashesMap)
        {
            allClashIds.Add(kvp.Key); // Adicionar o piso
            allClashIds.AddRange(kvp.Value); // Adicionar elementos em conflito
        }

        // Selecionar todos os elementos em conflito na vista ativa
        uiDoc.Selection.SetElementIds(allClashIds);

        // Opcionalmente, você pode destacar os elementos usando cores
        using (var transaction = new Transaction(uiDoc.Document, "Destacar Conflitos"))
        {
            transaction.Start();

            // Aplicar override gráfico nos elementos conflitantes
            var view = uiDoc.ActiveView;
            var overrideSettings = new OverrideGraphicSettings();
            overrideSettings.SetProjectionLineColor(new Color(255, 0, 0)); // Vermelho
            overrideSettings.SetSurfaceForegroundPatternColor(new Color(255, 0, 0));

            foreach (var clashId in allClashIds) view.SetElementOverrides(clashId, overrideSettings);

            transaction.Commit();
        }

        // Mostrar mensagem de orientação ao usuário
        TaskDialog.Show("Visualização de Conflitos",
            "Os elementos em conflito foram selecionados e destacados em vermelho na vista ativa.\n\n" +
            "Os destaques serão removidos quando você fechar esta vista ou usar o comando 'Remover Sobreposições'.");
    }

    /// <summary>
    ///     Exporta um relatório detalhado dos conflitos
    /// </summary>
    private void ExportClashReport(Document doc, Dictionary<ElementId, List<ElementId>> floorClashesMap)
    {
        var saveDialog = new SaveFileDialog
        {
            Title = "Salvar Relatório de Conflitos",
            Filter = "Arquivos CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = "Relatorio_Conflitos_Pisos.csv"
        };

        if (saveDialog.ShowDialog() != true)
            return;

        var reportBuilder = new StringBuilder();

        // Cabeçalho do relatório
        reportBuilder.AppendLine("ID do Piso,Nome do Piso,ID do Elemento,Categoria do Elemento,Nome do Elemento");

        // Gerar linhas do relatório
        foreach (var kvp in floorClashesMap)
        {
            var floorId = kvp.Key;
            var floor = doc.GetElement(floorId);
            var floorName = floor.Name;
            if (string.IsNullOrEmpty(floorName))
                floorName = "Sem Nome";

            foreach (var elementId in kvp.Value)
            {
                var element = doc.GetElement(elementId);
                var elementCategory = element.Category?.Name ?? "Sem Categoria";
                var elementName = element.Name;
                if (string.IsNullOrEmpty(elementName))
                    elementName = "Sem Nome";

                reportBuilder.AppendLine(
                    $"{floorId.IntegerValue},\"{floorName}\",{elementId.IntegerValue},\"{elementCategory}\",\"{elementName}\"");
            }
        }

        // Salvar o relatório
        File.WriteAllText(saveDialog.FileName, reportBuilder.ToString());

        // Informar ao usuário
        TaskDialog.Show("Exportação Concluída",
            $"O relatório de conflitos foi salvo em:\n{saveDialog.FileName}");
    }

    /// <summary>
    ///     Encontra elementos em colisão em um documento vinculado usando uma estratégia otimizada de duas etapas.
    /// </summary>
    /// <param name="hostElement">O elemento no documento hospedeiro.</param>
    /// <param name="linkInstance">A instância do vínculo a ser verificada.</param>
    /// <returns>Uma lista de ElementIds dos elementos em colisão.</returns>
    public List<ElementId> FindClashesInLink_Optimized(Element hostElement, RevitLinkInstance linkInstance)
    {
        var clashResults = new List<ElementId>();
        var linkDoc = linkInstance.GetLinkDocument();
        if (linkDoc == null) return clashResults;

        // Obter sólido do hospedeiro e transformação do vínculo
        var hostSolid = GetSolidFromElement(hostElement);
        if (hostSolid == null) return clashResults;

        var transform = linkInstance.GetTotalTransform();
        var inverseTransform = transform.Inverse;

        // --- ETAPA 1: FILTRAGEM GROSSEIRA COM BOUNDINGBOX ---

        // Obter a caixa delimitadora do hospedeiro e transformá-la para o espaço do vínculo
        var hostBBox = hostElement.get_BoundingBox(null);
        var linkSpaceOutline = TransformBoundingBox(hostBBox, inverseTransform);

        var bboxFilter = new BoundingBoxIntersectsFilter(linkSpaceOutline);

        // Coletar candidatos iniciais no documento vinculado
        var candidateCollector = new FilteredElementCollector(linkDoc);
        candidateCollector.WherePasses(bboxFilter);
        IList<Element> candidateElements = candidateCollector.ToElements();

        if (candidateElements.Count == 0) return clashResults;

        // --- ETAPA 2: FILTRAGEM FINA COM SÓLIDOS ---

        // Transformar o sólido do hospedeiro para o espaço do vínculo (apenas uma vez)
        var transformedHostSolid = SolidUtils.CreateTransformed(hostSolid, inverseTransform);

        // Iterar APENAS sobre os candidatos
        foreach (var candidate in candidateElements)
        {
            var candidateSolid = GetSolidFromElement(candidate);
            if (candidateSolid == null) continue;

            // Usar a operação booleana para uma verificação de interseção precisa
            try
            {
                var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    transformedHostSolid, candidateSolid, BooleanOperationsType.Intersect);

                if (intersection != null && intersection.Volume > 1e-9) // Usar uma pequena tolerância para o volume
                    clashResults.Add(candidate.Id);
            }
            catch (InvalidOperationException)
            {
            }
        }

        return clashResults;
    }

    /// <summary>
    ///     Transforma uma BoundingBoxXYZ aplicando uma transformação a seus cantos.
    /// </summary>
    private Outline TransformBoundingBox(BoundingBoxXYZ bbox, Transform transform)
    {
        var pt1 = transform.OfPoint(bbox.Min);
        var pt2 = transform.OfPoint(new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z));
        //... transformar todos os 8 pontos...
        var pt8 = transform.OfPoint(bbox.Max);

        // Encontrar os novos min/max para criar o Outline
        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        // Lógica para iterar sobre os 8 pontos e encontrar os novos min/max
        //...

        return new Outline(new XYZ(minX, minY, minZ), new XYZ(maxX, maxY, maxZ));
    }

    /// <summary>
    ///     Função auxiliar para extrair a primeira geometria sólida e não vazia de um elemento.
    /// </summary>
    private Solid GetSolidFromElement(Element element)
    {
        var options = new Options { ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine };
        var geomElem = element.get_Geometry(options);
        if (geomElem == null) return null;

        foreach (var geomObj in geomElem)
        {
            if (geomObj is Solid solid && solid.Volume > 0) return solid;
            // Também pode ser necessário iterar sobre GeometryInstance para encontrar sólidos aninhados.
            if (geomObj is GeometryInstance geomInst)
                foreach (var nestedGeomObj in geomInst.GetInstanceGeometry())
                    if (nestedGeomObj is Solid nestedSolid && nestedSolid.Volume > 0)
                        return nestedSolid;
        }

        return null;
    }
}