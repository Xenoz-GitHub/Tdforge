# Td# (TwoDeeSharp) — Agent Memory

## Project Root
`C:\TwoDeeSharp\`

## Source Code
- `C:\TwoDeeSharp\web\` — React + TypeScript web editor (Vite, Monaco, Canvas 2D)
- `C:\TwoDeeSharp\src\` — C# reference implementations (core, engine, tests)

## Reference Projects (C#, src/)
| Project | Path | Purpose |
|---------|------|---------|
| `TdSharp.Core` | `src\TdSharp.Core\` — Core language (Lexer, Parser, Interpreter, AST, TdValue, Environment) |
| `TdSharp.Engine` | `src\TdSharp.Engine\` — IGameEngine interface, GameEngine (Raylib), C++ bridge reference |
| `TdSharp.Tests` | `src\TdSharp.Tests\` — xUnit test patterns for TS port |

## Web Editor (web/)
| Directory | Content |
|-----------|---------|
| `web/src/components/` | React components (MenuBar, WorkspaceTabs, Toolbar2D, SplitLayout, SceneTree, FileTree, Outline, InspectorPanel, ConsolePanel, StatusBar, GamePreview, Icon) |
| `web/src/hooks/` | Custom hooks (useGameLoop, useCommands) |
| `web/src/contexts/` | React contexts (EditorContext, SceneContext, ConsoleContext, GameContext) |
| `web/src/assets/icons/` | 42 Godot SVG icons |
| `web/docs/` | Language reference |
| `web/src/core/` | Td# TS core (Lexer, Parser, Interpreter — planned) |

## Git Repo (initialized)
`C:\TwoDeeSharp.git` (bare, for push target)

## External Resources (MIT License)

### Godot Editor Icons (16×16 SVG)
Source: `https://raw.githubusercontent.com/godotengine/godot/master/editor/icons/`
Target: `C:\TwoDeeSharp\web\src\assets\icons\`

Required files (42 SVGs, all downloaded):
- `ToolSelect.svg`, `ToolMove.svg`, `ToolRotate.svg`, `ToolScale.svg` — transform tools
- `2D.svg`, `Script.svg`, `AssetLib.svg` (← AssetStore) — workspace tabs
- `Play.svg`, `Stop.svg`, `Pause.svg`, `Reload.svg` — playback controls
- `SnapGrid.svg`, `GridView.svg` (← GridToggle), `ZoomIn.svg` (← ViewportZoom), `ZoomOut.svg` (← ViewportZoom), `PanView.svg` (← ToolPan) — viewport
- `Sprite.svg` (← Sprite2D), `Animation.svg`, `Audio.svg` (← AudioStreamPlayer), `Node.svg`, `Camera2D.svg`, `CollisionShape2D.svg`, `RigidBody2D.svg`, `StaticBody2D.svg`, `Area2D.svg`, `TileMap.svg`, `Light2D.svg` (← PointLight2D), `Particles2D.svg` (← GPUParticles2D), `Label.svg`, `Control.svg`, `ColorRect.svg` — common node icons
- `Import.svg` (← ImportCheck), `Export.svg` (← Load), `Save.svg`, `New.svg` — file actions
- `Folder.svg`, `File.svg`, `Search.svg`, `Clear.svg`, `Duplicate.svg`, `Remove.svg` — misc UI
- `Godot.svg` — app icon

### Godot Demo Projects (MIT License)
Source: `https://github.com/godotengine/godot-demo-projects`
Useful folders:
- `mono/dodge_the_creeps/` — C# patterns (Player.cs, Mob.cs, HUD.cs, Main.cs)
- `plugins/` — Editor plugin architecture (main screen, custom dock, custom node, import)
- `gui/control_gallery/` — Full UI control reference
- `gui/drag_and_drop/` — Drag-drop patterns
- `gui/theming_override/` — Theme porting reference
- `2d/` — Gameplay function examples (platformer, physics, nav, particles, etc.)

## Architecture
- Td# source → Parser (AST) → Interpreter (Canvas 2D render)
- Web editor: React UI → Monaco (code editing) → Td# TS Core (parse/execute) → Canvas 2D (game render)

## Known Issues / Fixed
| Issue | Fix |
|-------|-----|
| Lexer `IndexOutOfRange` on trailing backslash in string | Added `IsAtEnd()` guard after `Advance()` |
| Lexer `double.Parse` crash on malformed number | Changed to `double.TryParse` with error |
| Parser `Synchronize()` `IndexOutOfRange` on `_tokens[-1]` | Added `IsAtEnd()` guard before `Advance()` |
| All ~40 Interpreter builtins crash on wrong arg count | Added `RequireArgs` validation to every builtin |
| `add_animation` NRE on non-array second argument | Added `TdValueType.Array` type check |
| `Environment.GetAt` throws `KeyNotFoundException` | Changed to `TryGetValue` |
| `Environment.Ancestor` NRE on over-deep distance | Added null check in loop |
| `ValuesEqual` compared Arrays/Maps by string | Changed to referential equality |
| GameEngine.Run resources leak on exception | Wrapped loop in `try-finally` |
| GameEngine camera `Math.Clamp` crash on small bounds | Guarded with `b.Width > ScreenWidth` check |
| GameEngine `Flash()` ignored duration parameter | Added flash state fields + timer + overlay draw |
| GameEngine sound/music volume not independent | Added per-channel volume fields |
| GameEngine music stream memory leak on track switch | Added `UnloadMusicStream` before replace |
| GameEngine `FillRectMap` crash on negative coords | Clamped start row/col to 0 |
| Transpiler constructor body produces invalid Lua (`self:var`) | Removed `self:` prefix from non-assignment stmts |
| CppNativeBridge path traversal in `extern()` | Added `Path.GetFullPath` + project dir prefix check |
| CppNativeBridge Marshal memory leak on exception | Added `try-finally` around `AllocHGlobal` |
| CppCompiler deadlock on large stderr output | Read stderr asynchronously |
| CppCompiler null `Process.Start` return | Added null check with `?? throw` |
| CppCompiler `UnauthorizedAccessException` on restricted dirs | Wrapped `GetFiles` in try-catch |
| Launcher `tds --lua` IndexOutOfRange on empty filtered args | Added `filtered.Length == 0` guard |
| Launcher fragile game mode detection (`source.Contains`) | Changed to `Regex.IsMatch` with `\b` |
| Launcher null file path prints literal `'null'` | Added `?? "<null>"` fallback |
| Launcher `Path.GetDirectoryName` returns null for root path | Added `?? Environment.CurrentDirectory` |
| Launcher `GameEngine` not disposed | Added `using` keyword |
| For double loop infinite loop over 2^53 (FP precision) | Added 100M iteration cap with `RuntimeException` |
| `clamp()` non-standard arg order | Changed to `Math.Clamp(value, min, max)` |

## Next Steps
- ~~Download Godot SVG icons → `Assets\Icons\`~~ **DONE**
- ~~Scaffold web/ with Vite + React + TypeScript~~ **DONE**
- ~~Create UI layout components (Godot 3-col)~~ **DONE**
- ~~Copy 42 SVG icons to web/~~ **DONE**
- ~~Port Td# language reference to docs/~~ **DONE**
- Port Td# core (Lexer, Parser, Interpreter) to TypeScript
- Add Monaco editor with Monarch grammar for Td#
- Wire Play button → run code → render to Canvas 2D
