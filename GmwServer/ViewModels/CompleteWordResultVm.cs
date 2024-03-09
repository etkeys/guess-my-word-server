
namespace GmwServer;

public class CompleteWordResultVm
{
    public CompleteWordResultVm(bool isGuessCorrect, bool hasSurrended){
        HasSurrended = hasSurrended;
        IsGuessCorrect = isGuessCorrect;
    }

    public bool HasSurrended {get;}
    public bool IsGuessCorrect {get;}
}