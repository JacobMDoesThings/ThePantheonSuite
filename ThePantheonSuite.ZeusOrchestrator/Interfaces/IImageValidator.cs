namespace ThePantheonSuite.ZeusOrchestrator.Interfaces;

public interface IImageValidator
{
    Task Validate(MemoryStream stream);
}