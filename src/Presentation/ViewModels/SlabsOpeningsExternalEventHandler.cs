using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ClashOpenings.src.Models;
using ClashOpenings.src.Services;

namespace ClashOpenings.src.Presentation.ViewModels;

/// <summary>
///     Manipulador de eventos externos para detecção de conflitos e criação de aberturas (furos) em lajes no Revit.
///     Implementa <see cref="IExternalEventHandler" /> para permitir operações assíncronas no Revit.
/// </summary>
public class SlabsOpeningsExternalEventHandler : IExternalEventHandler
{
    private RevitLinkInstance? _linkInstance1;
    private RevitLinkInstance? _linkInstance2;
    private Action<string>? _updateStatusCallback;

    /// <summary>
    ///     Executa a lógica de detecção de conflitos e criação de aberturas.
    ///     Este método é chamado em uma transação do Revit e deve conter todas as operações no modelo.
    /// </summary>
    /// <param name="app">A aplicação UI do Revit.</param>
    public void Execute(UIApplication app)
    {
        try
        {
            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;

            if (!InitializeAndValidate(doc)) return;

            var searchVolume = GetSearchVolume(doc);
            if (searchVolume == null)
            {
                _updateStatusCallback?.Invoke(
                    "Erro: Tipo de vista não suportado. Use uma vista de planta baixa ou uma vista 3D," +
                    " com a região de recorte e a caixa de corte ativa.");
                return;
            }

            var (linkElementData1, linkElementData2) = CollectLinkElements(doc, searchVolume);

            var clashResults = DetectClashes(linkElementData1, linkElementData2);

            var openingsCreated = CreateOpenings(doc, clashResults);

            UpdateStatusMessage(clashResults.Count, openingsCreated);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    /// <summary>
    ///     Retorna o nome do manipulador de eventos externo.
    /// </summary>
    /// <returns>O nome do manipulador.</returns>
    public string GetName()
    {
        return "Clash Detection External Event Handler";
    }

    /// <summary>
    ///     Define as instâncias dos modelos vinculados a serem usados na detecção de conflitos.
    /// </summary>
    /// <param name="link1">A primeira instância de modelo vinculado.</param>
    /// <param name="link2">A segunda instância de modelo vinculado.</param>
    public void SetLinks(RevitLinkInstance? link1, RevitLinkInstance? link2)
    {
        _linkInstance1 = link1;
        _linkInstance2 = link2;
    }

    /// <summary>
    ///     Define um callback para atualizar a mensagem de status na interface do usuário.
    /// </summary>
    /// <param name="callback">A ação de callback que recebe uma string com a mensagem de status.</param>
    public void SetStatusCallback(Action<string>? callback)
    {
        _updateStatusCallback = callback;
    }

    /// <summary>
    ///     Inicializa e valida as instâncias de link selecionadas.
    /// </summary>
    /// <param name="doc">O documento atual do Revit.</param>
    /// <returns>True se a inicialização e validação forem bem-sucedidas; caso contrário, false.</returns>
    private bool InitializeAndValidate(Document doc)
    {
        if (_linkInstance1 == null || _linkInstance2 == null)
        {
            _updateStatusCallback?.Invoke("Erro: Por favor, selecione os DOIS modelos e tente novamente.");
            return false;
        }

        if (_linkInstance1.Id == _linkInstance2.Id)
        {
            _updateStatusCallback?.Invoke("Erro: O Mesmo modelo foi selecionado duas vezes. " +
                                          "Por favor, tente novamente com modelos diferentes.");
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Obtém o volume de busca da vista atual do Revit.
    /// </summary>
    /// <param name="doc">O documento atual do Revit.</param>
    /// <returns>O <see cref="Outline" /> que representa o volume de busca ou null se a vista não for suportada.</returns>
    private Outline GetSearchVolume(Document doc)
    {
        var viewService = new ViewGeometryService(doc);
        return viewService.GetSearchVolume();
    }

    /// <summary>
    ///     Coleta elementos dos modelos vinculados dentro do volume de busca especificado.
    /// </summary>
    /// <param name="doc">O documento atual do Revit.</param>
    /// <param name="searchVolume">O volume de busca para coletar elementos.</param>
    /// <returns>Uma tupla contendo <see cref="LinkElementData" /> para cada um dos dois links.</returns>
    private (LinkElementData linkElementData1, LinkElementData linkElementData2) CollectLinkElements(
        Document doc, Outline searchVolume)
    {
        var elements1 = ElementCollector.GetElementsFromLink(doc, _linkInstance1, searchVolume);
        var linkElementData1 = new LinkElementData(_linkInstance1!, elements1);

        var elements2 = ElementCollector.GetElementsFromLink(doc, _linkInstance2!, searchVolume);
        var linkElementData2 = new LinkElementData(_linkInstance2!, elements2);

        return (linkElementData1, linkElementData2);
    }

    /// <summary>
    ///     Executa a detecção de conflitos entre os elementos dos links.
    /// </summary>
    /// <param name="linkElementData1">Dados dos elementos do primeiro link.</param>
    /// <param name="linkElementData2">Dados dos elementos do segundo link.</param>
    /// <returns>Uma lista de <see cref="ClashResult" /> contendo os conflitos encontrados.</returns>
    private List<ClashResult> DetectClashes(LinkElementData linkElementData1, LinkElementData linkElementData2)
    {
        var clashDetector = new ClashDetective();
        return clashDetector.FindClashes(linkElementData1, linkElementData2);
    }

    /// <summary>
    ///     Cria as furações (aberturas) no documento do Revit com base nos resultados dos conflitos.
    /// </summary>
    /// <param name="doc">O documento atual do Revit.</param>
    /// <param name="clashResults">A lista de <see cref="ClashResult" /> contendo os conflitos.</param>
    /// <returns>O número de furações criadas com sucesso.</returns>
    private int CreateOpenings(Document doc, List<ClashResult> clashResults)
    {
        var familyPlacer = new FamilyCreator(doc);
        return familyPlacer.CreateOpenings(clashResults);
    }

    /// <summary>
    ///     Atualiza a mensagem de status na interface do usuário com um resumo dos resultados.
    /// </summary>
    /// <param name="clashCount">O número total de conflitos encontrados.</param>
    /// <param name="openingsCreated">O número de aberturas criadas.</param>
    private void UpdateStatusMessage(int clashCount, int openingsCreated)
    {
        var summary = new StringBuilder();
        summary.AppendLine($"Detecção de conflitos completa. Encontramos {clashCount} conflitos.");
        if (openingsCreated > 0) summary.AppendLine($"{openingsCreated} aberturas foram criadas com sucesso.");
        _updateStatusCallback?.Invoke(summary.ToString());
    }

    /// <summary>
    ///     Lida com exceções, exibindo uma mensagem de erro para o usuário.
    /// </summary>
    /// <param name="ex">A exceção ocorrida.</param>
    private void HandleException(Exception ex)
    {
        _updateStatusCallback?.Invoke($"Erro: {ex.Message}");
        TaskDialog.Show("Erro", $"Falha na execução da detecção de conflitos: {ex.Message}");
    }
}