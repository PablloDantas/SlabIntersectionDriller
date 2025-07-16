namespace ClashOpenings.Presentation.RevitSetup.Ribbon;

/// <summary>
///     Representa os dados necessários para criar um botão de ação (PushButton) na Ribbon do Autodesk Revit.
/// </summary>
/// <param name="Name">O nome único interno do botão, usado para identificação programática.</param>
/// <param name="Text">O texto que será exibido no botão na interface do usuário do Revit.</param>
/// <param name="CommandNamespace">
///     O namespace completo e o nome da classe do comando externo (IExternalCommand)
///     a ser executado quando o botão for clicado.
/// </param>
/// <param name="Tooltip">O texto da dica de ferramenta que aparece ao passar o mouse sobre o botão.</param>
/// <param name="IconName">O nome do arquivo do ícone (sem a extensão) associado ao botão. O ícone deve estar incorporado.</param>
public record ButtonData(
    string Name,
    string Text,
    string CommandNamespace,
    string Tooltip,
    string IconName
);