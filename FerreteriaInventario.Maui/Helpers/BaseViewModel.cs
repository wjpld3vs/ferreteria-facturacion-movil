namespace FerreteriaInventario.Maui.Helpers;

public abstract class BaseViewModel : ObservableObject
{
    private bool _isBusy;
    private string _title = string.Empty;
    private string _errorMessage = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    protected async Task RunSafeAsync(Func<Task> action, string? successMessage = null)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            await action();

            if (!string.IsNullOrWhiteSpace(successMessage))
            {
                await ShowMessageAsync("Proceso completado", successMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await ShowMessageAsync("Atencion", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task ShowMessageAsync(string title, string message)
    {
        if (Shell.Current is null)
        {
            return;
        }

        await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert(title, message, "Aceptar"));
    }

    protected async Task<bool> ConfirmAsync(string title, string message)
    {
        if (Shell.Current is null)
        {
            return false;
        }

        return await MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlert(title, message, "Si", "No"));
    }
}
