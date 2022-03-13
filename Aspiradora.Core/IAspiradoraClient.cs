using Aspiradora.Web.Models;

namespace Aspiradora.Core;
public interface IAspiradoraClient
{
    Task ReceiveCleanerAsync(string cleaner);
    Task ReceiveStartAsync(string sessionId, string places, string cleaners);
    Task ReceiveRotationAsync(string id, Rotation rotation);
    Task ReceiveListAsync(string places);
    Task ReceiveCellChangedAsync(int row, int column, PlaceState state);
    Task CleanerRowChangedAsync(string id, int rowIndex);
    Task CleanerColumnChangedAsync(string id, int columnIndex);
    Task ReceiveDisconnectAsync(string id);
    Task ReceiveMessageAsync(string nickName, string date, string text, int type);
    Task ReceiveErrorAsync(string message);
    Task ReceiveScoreAsync(string id, int score);
    Task ReceiveWinnerAsync(string id);
}
