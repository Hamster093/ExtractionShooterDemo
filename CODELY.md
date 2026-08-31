

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-08-31 16:22:47] [project] 视觉验证方法：本机无 ffmpeg，exec_runtime_script 的 record_game_view (MP4) 会失败；改用 exec_runtime_script 协程 + ScreenCapture.CaptureScreenshotAsTexture 保存 PNG 到 screenshots/ 再用 analyze_multimedia 检查（协程需用 System.Collections.IEnumerator，默认导入含泛型 IEnumerator<T> 会编译冲突）。Why: 2026-08-31 HUD 任务中 MP4 录制因缺 ffmpeg 失败。How to apply: 每次需要 Game View 视觉验证时直接走 PNG 截图方案。
- [2026-08-31 17:34:58] [project] 项目已重组：UI 美术资源移到 Assets/Resources/Art/UI/（含 Icons/），预制体约定在 Assets/Resources/Prefabs/；旧的 Assets/Art/ 与 Assets/Prefabs/ 路径已失效（2026-08-31 之前的 HUD 任务生成的路径已变化）。Why: 2026-08-31 战利品任务中发现 Assets/Art 不存在、精灵在 Resources 下。How to apply: 后续引用 UI 精灵/预制体资产时用 Resources 下新路径，或先 glob 搜索确认实际位置。

### Reference

