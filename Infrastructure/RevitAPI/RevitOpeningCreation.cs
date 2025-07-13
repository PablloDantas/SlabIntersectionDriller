using Autodesk.Revit.DB;
using ClashOpenings.Core.Application.Interfaces;

namespace ClashOpenings.Infrastructure.RevitAPI;

public class RevitOpeningCreation : IOpeningService<FamilyInstance>
{
    public FamilyInstance CreateOpening()
    {
        throw new NotImplementedException();
    }
}