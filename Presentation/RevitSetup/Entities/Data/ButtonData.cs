namespace ClashOpenings.Presentation.RevitSetup.Entities.Data;

/// <summary>
/// Contém todas as informações necessárias para criar um PushButton na Ribbon do Revit.
/// </summary>
public record ButtonData(
    string Name,
    string Text,
    string CommandNamespace,
    string Tooltip,
    string IconName
);
