public interface IUIPanel
{
    abstract UIPriority Priority { get; }
    void OnOpen();
    void OnClose();

    /// <summary>
    /// 每个面板自定义自己的 ESC 行为
    /// </summary>
    void OnEscapePressed();
}