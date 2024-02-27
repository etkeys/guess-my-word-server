
namespace GmwServer;

public class WordWithDefinitionsVm
{
    public string Word {get; init;} = string.Empty;
    public PartOfSpeech PartOfSpeech {get; init;}
    public IEnumerable<DefinitionVm> Definitions {get; init;} = new List<DefinitionVm>();

}