# Td# Language Bible

> **Version 1.0** — The Td# scripting language for 2D game development.

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Getting Started](#2-getting-started)
3. [Syntax Conventions](#3-syntax-conventions)
4. [Comments](#4-comments)
5. [Variables and Constants](#5-variables-and-constants)
6. [Data Types](#6-data-types)
    - 6.1 Nil
    - 6.2 Bool
    - 6.3 Number
    - 6.4 String
    - 6.5 Array
    - 6.6 Map
    - 6.7 Vector2
    - 6.8 Rect
    - 6.9 Color
    - 6.10 Sprite
7. [Operators](#7-operators)
    - 7.1 Arithmetic
    - 7.2 Comparison
    - 7.3 Logical
    - 7.4 String Concatenation
    - 7.5 Length Operator
    - 7.6 Compound Assignment
    - 7.7 Precedence Table
8. [Control Flow](#8-control-flow)
    - 8.1 If / Elif / Else
    - 8.2 While Loop
    - 8.3 For Loop
    - 8.4 For-Each Loop
    - 8.5 Match Expression
    - 8.6 Break / Continue
9. [Functions](#9-functions)
10. [Classes](#10-classes)
11. [Event Functions](#11-event-functions)
12. [Built-in Functions](#12-built-in-functions)
    - 12.1 Console
    - 12.2 Math
    - 12.3 Graphics
    - 12.4 Input
    - 12.5 Game Flow
    - 12.6 Audio
    - 12.7 Visual Effects
    - 12.8 Camera
    - 12.9 Animation
    - 12.10 Procedural Generation
    - 12.11 Debug
    - 12.12 File I/O
    - 12.13 Type Constructors
    - 12.14 Extended
13. [Built-in Methods](#13-built-in-methods)
14. [Import System](#14-import-system)
15. [Td# → Canvas 2D Mapping](#15-td--canvas-2d-mapping)
16. [Language Aliases](#16-language-aliases)
17. [Error Handling](#17-error-handling)
18. [Examples](#18-examples)

---

## 1. Introduction

Td# (TwoDeeSharp) is a custom scripting language for 2D game development. It features a C-like syntax with English-like keyword aliases, first-class 2D types (Vector2, Rect, Color, Sprite), and a rich set of built-in functions for graphics, audio, input, and game logic.

Td# runs via an interpreter written in TypeScript (web) or C# (desktop). In the web editor, all graphics built-ins map directly to Canvas 2D API calls.

---

## 2. Getting Started

Minimal game:

```
function start()
    say("Hello, Td#!")
end

function tick(dt)
    clear(#1a1a2e)
    circle(320, 240, 50, Color(255, 100, 100))
end
```

Console mode (no event functions — runs top-to-bottom):

```
var name = "World"
say("Hello, " .. name .. "!")

for i = 1 to 5 do
    say("Count: " .. i)
end
```

---

## 3. Syntax Conventions

- Statements are separated by newlines (not semicolons).
- Blocks are delimited by keywords: `if` ... `end`, `function` ... `end`, etc.
- Strings use double quotes: `"hello"`
- Comments use `//` or `/* */`
- Indices are 0-based for arrays, 1-based for strings.
- Case-sensitive.

---

## 4. Comments

```
// Single line comment

/* Block comment
   spanning multiple lines */

/* Nested /* block */ comments are supported */
```

---

## 5. Variables and Constants

```
var x = 5                 // Mutable variable
var name = "Alice"        // String variable
local temp = 10            // Local-scope variable
const PI = 3.14159         // Immutable constant

x = 10                     // Assignment to existing variable
y = 20                     // Auto-creates global if not declared
```

**Rules:**
- `var` declares a variable (default scope is block or global).
- `local` declares a block-scoped variable.
- `const` declares an immutable constant (must be initialized).
- Assigning to an undeclared name **auto-creates** it as a global.

---

## 6. Data Types

### 6.1 Nil

Represents the absence of a value.

```
var empty = nil
```

**Truthiness:** Always `false`.

### 6.2 Bool

Boolean values `true` and `false`.

```
var flag = true
var done = false
```

**Truthiness:** `true` if value is `true`.

### 6.3 Number

64-bit floating point (double precision).

```
var a = 42           // Integer
var b = 3.14159      // Decimal
var c = -7.5         // Negative
```

**Truthiness:** `false` if `0`, `true` otherwise.

### 6.4 String

Immutable text enclosed in double quotes.

```
var msg = "Hello, World!"
```

**Escape sequences:**
| Sequence | Result |
|----------|--------|
| `\n` | Newline |
| `\t` | Tab |
| `\\` | Backslash |
| `\"` | Double quote |

**String indexing** (1-based):
```
var s = "hello"
s[1]    // "h"
s[#s]   // "o" (last character)
```

**Truthiness:** `false` if empty string `""`, `true` otherwise.

### 6.5 Array

Ordered collection of values.

```
var arr = [1, 2, 3, 4, 5]
var mixed = [1, "hello", true, [4, 5]]
```

**Array methods:**
| Method | Description |
|--------|-------------|
| `arr.push(val)` | Append value, returns new length |
| `arr.pop()` | Remove and return last element |

**Indexing** (0-based):
```
arr[0]   // first element
arr[#arr - 1]  // last element
```

**Truthiness:** Always `true`.

### 6.6 Map

Key-value dictionary (string keys only).

```
var map = {name = "Alice", score = 100}
```

**Access:**
```
map["name"]     // "Alice"
map["score"]    // 100
```

**Truthiness:** Always `true`.

### 6.7 Vector2

2D vector with `x`, `y` components.

```
var v = Vector2(10, 20)
v.x     // 10
v.y     // 20
```

**Truthiness:** Always `true`.

### 6.8 Rect

Axis-aligned rectangle with `x`, `y`, `w`, `h`.

```
var r = Rect(0, 0, 640, 480)
r.x     // 0
r.y     // 0
r.w     // 640
r.h     // 480
```

**Rect method:**
| Method | Description |
|--------|-------------|
| `r.hits(other)` | AABB collision test, returns `bool` |

**Truthiness:** Always `true`.

### 6.9 Color

RGBA color with `r`, `g`, `b`, `a` (0-255).

```
var c = Color(255, 0, 0)          // Red, alpha=255
var c2 = Color(255, 0, 0, 128)    // Red, alpha=128
```

**Hex color literal:**
```
clear(#1a1a2e)    // 6-digit hex
clear(#FF0)       // 3-digit hex (expands to #FFFF00)
```

**Color components:**
```
c.r     // 0-255
c.g     // 0-255
c.b     // 0-255
c.a     // 0-255
```

**Truthiness:** Always `true`.

### 6.10 Sprite

Reference to a sprite image by path.

```
var s = Sprite("player.png")
s.path      // "player.png"
```

**Truthiness:** Always `true`.

---

## 7. Operators

### 7.1 Arithmetic

| Operator | Description |
|----------|-------------|
| `+` | Addition |
| `-` | Subtraction / Negation |
| `*` | Multiplication |
| `/` | Division |
| `%` | Modulo |
| `^` | Power (right-associative) |

### 7.2 Comparison

| Operator | Description |
|----------|-------------|
| `==` | Equal |
| `!=` | Not equal |
| `>` | Greater than |
| `>=` | Greater or equal |
| `<` | Less than |
| `<=` | Less or equal |

### 7.3 Logical

| Operator | Description |
|----------|-------------|
| `and` | Logical AND (short-circuit) |
| `or` | Logical OR (short-circuit) |
| `not` | Logical NOT |

### 7.4 String Concatenation

```
var msg = "Hello, " .. name .. "!"
```

The `..` operator concatenates two values as strings.

### 7.5 Length Operator

The `#` prefix operator returns:
- Array: element count
- Map: entry count
- String: character count

```
#arr      // array length
#map      // map entry count
#"hello"  // 5
```

### 7.6 Compound Assignment

Shorthand for modifying a variable:

| Operator | Equivalent to |
|----------|---------------|
| `x += y` | `x = x + y` |
| `x -= y` | `x = x - y` |
| `x *= y` | `x = x * y` |
| `x /= y` | `x = x / y` |

### 7.7 Precedence Table

(Lowest to highest precedence)

| Level | Operators | Associativity |
|-------|-----------|---------------|
| 1 | `=` | Right |
| 2 | `or` | Left |
| 3 | `and` | Left |
| 4 | `==` `!=` `>` `>=` `<` `<=` | Left |
| 5 | `+` `-` `..` | Left |
| 6 | `*` `/` `%` | Left |
| 7 | `^` | Right |
| 8 | `-` (unary) `not` `#` | Right |

---

## 8. Control Flow

### 8.1 If / Elif / Else

```
if condition then
    // body
end

if condition then
    // body
else
    // else body
end

if condition then
    // body
elif other then
    // elif body
else
    // else body
end
```

`elif` can chain multiple conditions. Conditions are **truthy** values (not just `true`).

### 8.2 While Loop

```
while condition do
    // body
end
```

Also valid with alias:

```
as_long_as condition do
    // body
end
```

### 8.3 For Loop

Numeric for loop:

```
for i = 1 to 10 do
    // body
end

for i = 0 to 100 step 5 do
    // body
end
```

Also valid with alias:

```
repeat i = 1 to 10 do
    // body
end
```

### 8.4 For-Each Loop

```
for item in collection do
    // body
end
```

### 8.5 Match Expression

```
match value
    1 -> say("one")
    2 -> say("two")
    else -> say("other")
end
```

Desugars to an if/elif/else chain.

### 8.6 Break / Continue

```
while true do
    if done then
        stop     // break
    end
    if skipFlag then
        skip     // continue
    end
end
```

Also valid with aliases:

| Keyword | Alias |
|---------|-------|
| `break` | `stop` |
| `continue` | `skip` |

---

## 9. Functions

### Declaration

```
function greet(name)
    say("Hello, " .. name .. "!")
    return 42
end
```

### Return

```
function add(a, b)
    return a + b
end
```

Also valid: `send_back a + b`.

### Default return value

Functions without an explicit `return` return `nil`.

### Calling

```
var result = greet("Alice")
var sum = add(3, 4)
```

### First-class functions

Functions can be assigned to variables and passed as arguments.

---

## 10. Classes

### Declaration

```
class Player copies Entity
    function constructor(x, y)
        this.x = x
        this.y = y
    end

    function move(dx, dy)
        this.x = this.x + dx
        this.y = this.y + dy
    end
end
```

Also valid with aliases:

```
blueprint Player copies Entity
    // ...
end
```

### Instantiation

```
var p = Player(100, 200)
p.move(10, 0)
```

### This keyword

Inside methods, `this` refers to the current instance.

### Inheritance

`copies` / `extends` establishes a parent-child class relationship. Child classes inherit methods from the parent.

### Key rules:
- `constructor` is the special initialization method (called on `ClassName(...)`).
- Methods are declared with `function methodName(...)` inside the class body.
- `this` is implicit — no special syntax needed.

---

## 11. Event Functions

When the interpreter detects certain function names, they are called automatically in game mode:

| Function | Called When | Parameters |
|----------|-------------|------------|
| `start()` | Once at startup | (none) |
| `tick(dt)` | Every frame | `dt` = delta time in seconds |
| `paint()` | Every frame after tick | (none) |
| `pressed(key)` | A key is pressed | `key` = key name string |
| `released(key)` | A key is released | `key` = key name string |
| `tap(button)` | Mouse button tapped | `button` = "left" or "right" |
| `move(x, y)` | Mouse cursor moved | `x`, `y` = coordinates |
| `bump(obj_a, obj_b)` | Two objects collide | `obj_a`, `obj_b` = object instances |

**Console mode:** If none of these functions exist, code runs sequentially from top to bottom.

---

## 12. Built-in Functions

### 12.1 Console

```
say(...)                     → nil    // Print arguments joined by space
show(text, x, y, size, color)→ nil    // Draw text on screen
inspect(value)               → nil    // Print value to console for debugging
```

### 12.2 Math

```
roll(min, max)                          → number  // Random in [min, max)
abs(x)                                  → number  // Absolute value
min(a, b)                               → number  // Minimum
max(a, b)                               → number  // Maximum
clamp(value, min, max)                  → number  // Clamp to [min, max]
dist(x1, y1, x2, y2)                    → number  // Euclidean distance
smooth(from, to, t)                     → number  // Lerp: from + (to - from) * t
noise(x, y)                             → number  // Simple 2D noise
```

### 12.3 Graphics

```
clear(color)                                        → nil  // Clear screen to color
show(sprite, x, y)                                  → nil  // Draw sprite at position
show(sprite, x, y, rotation)                        → nil  // Draw with rotation
show(sprite, x, y, rotation, scaleX, scaleY)        → nil  // Draw with scale
show(sprite, x, y, rotation, scaleX, scaleY, tint)  → nil  // Draw with tint color
rect(x, y, w, h, color)                            → nil  // Filled rectangle
circle(x, y, radius, color)                        → nil  // Filled circle
line(x1, y1, x2, y2, color)                       → nil  // Line
tilemap(data, camX, camY)                          → nil  // 2D tile grid render
```

### 12.4 Input

```
holding(key)     → bool    // Is key currently pressed?
tap(button)      → bool    // Was mouse button tapped? ("left", "right")
mouse_x()        → number  // Current mouse X position
mouse_y()        → number  // Current mouse Y position
gamepad_held(button)→ bool  // Gamepad button down ("A", "B", "X", "Y", "START", "SELECT")
gamepad_axis(axis) → number // Gamepad axis ("left_x", "left_y", "right_x", "right_y")
```

### 12.5 Game Flow

```
go_to()   → nil  // Reload/restart the game
quit()    → nil  // Exit the application
spawn(obj)→ obj  // Clone an object instance
remove()  → nil  // (stub — currently performs no action)
reload()  → nil  // Restart game loop (sets running flag to false)
```

### 12.6 Audio

```
play_sound(path)         → nil  // Play one-shot sound effect
play_music(path)         → nil  // Play music stream (replaces current track)
stop_music()             → nil  // Stop currently playing music
set_sound_volume(vol)    → nil  // 0.0 (silent) to 1.0 (full)
set_music_volume(vol)    → nil  // 0.0 (silent) to 1.0 (full)
is_playing(path)         → bool // Is the given music track playing?
```

### 12.7 Visual Effects

```
emit(type, x, y, count)               → nil  // Particles: "fire", "smoke", "rain", "sparkle"
shake(amplitude, duration)            → nil  // Screen shake
fade_in(duration, color)              → nil  // Fade in from color
fade_out(duration, color)             → nil  // Fade out to color
flash(color, duration)                → nil  // Screen flash overlay
tween(target, property, to, duration) → nil  // Animate property (immediate in current impl)
trail(spritePath, enabled)            → nil  // Trail effect (stub)
tint(sprite, color)                   → nil  // Apply tint to sprite
```

### 12.8 Camera

```
camera_follow(target, smoothness)  → nil     // Follow object or Vector2
camera_zoom(zoom)                  → nil     // Set zoom level
camera_rotate(degrees)             → nil     // Set rotation in degrees
camera_bounds(rect)                → nil     // Set camera bounds
world_to_screen(worldX, worldY)    → Vector2  // Convert world to screen coords
```

### 12.9 Animation

```
animate(sprite, animName, dt)     → nil     // Play animation (stub)
add_animation(name, frames, speed)→ nil     // Register animation (stub)
get_frame(sprite)                 → number  // Current frame index (stub → 0)
set_frame(sprite, frame)          → nil     // Set current frame (stub)
animation_finished(sprite)        → bool    // Is animation done? (stub → false)
```

### 12.10 Procedural Generation

```
grid(w, h)                           → array   // Create w×h 2D array of zeros
fill_rect_map(map, x, y, w, h, tile) → nil     // Fill rectangle region in tilemap
find_path(x1, y1, x2, y2)            → array   // Pathfinding (stub — returns 2-point path)
path_length(path)                    → number  // Sum of Vector2 distances along path
time_scale()                         → number  // Get current time scale factor
```

### 12.11 Debug

```
draw_hitbox(enabled)  → nil  // Toggle hitbox overlay rendering
show_fps(enabled)     → nil  // Toggle FPS counter display
profile(name)         → nil  // Profile a named section (stub)
breakpoint()          → nil  // Trigger debugger breakpoint
step_frame()          → nil  // Advance one frame in step mode (stub)
```

### 12.12 File I/O

```
read_file(path)         → string      // Read entire text file
write_file(path, content) → nil       // Write text file (overwrites)
append_file(path, content) → nil      // Append text to file
file_exists(path)       → bool        // Check if file exists
save_data(path, data)   → nil         // Save data (string serialization)
load_data(path)         → string|nil  // Load data, returns nil if missing
```

### 12.13 Type Constructors

```
Vector2(x, y)                     → Vector2
Rect(x, y, w, h)                  → Rect
Color(r, g, b [, a])             → Color   // a defaults to 255
Sprite(path)                      → Sprite
```

### 12.14 Extended

```
after(seconds)     → number  // Timer (stub — returns the seconds argument)
timer(seconds)     → number  // Create/get a timer (stub — returns seconds)
timer_elapsed(id)  → bool    // Has the timer elapsed? (stub → false)
```

---

## 13. Built-in Methods

Methods available on runtime types:

| Type | Method | Description |
|------|--------|-------------|
| Array | `arr.push(val)` | Append value, returns new length |
| Array | `arr.pop()` | Remove and return last element |
| Rect | `r.hits(other)` | AABB collision test with another Rect, returns `bool` |

---

## 14. Import System

```
import "path/to/file.td"
```

Imports are **host-handled** — the runtime loads and executes the target file before continuing. Circular imports are not detected.

---

## 15. Td# → Canvas 2D Mapping

When running on the web editor, each graphics built-in maps to Canvas 2D API calls:

| Td# Built-in | Canvas 2D Implementation |
|-------------|--------------------------|
| `clear(color)` | `ctx.fillStyle` + `ctx.fillRect(0, 0, w, h)` |
| `show(sprite, x, y, ...)` | `ctx.drawImage(img, x, y)` |
| `rect(x, y, w, h, color)` | `ctx.fillStyle` + `ctx.fillRect` |
| `circle(x, y, radius, color)` | `ctx.arc(x, y, radius, 0, 2*PI)` + `ctx.fill` |
| `line(x1, y1, x2, y2, color)` | `ctx.beginPath` + `ctx.moveTo/lineTo` + `ctx.stroke` |
| `tilemap(data, camX, camY)` | Loop over grid, `ctx.drawImage` per tile |
| `vector2(x, y)` | Plain `{x, y}` object |
| `rect(x, y, w, h)` | Plain `{x, y, w, h}` object |
| `color(r, g, b, a)` | CSS `rgba(r, g, b, a/255)` string |
| `shake(amp, dur)` | Screen offset jitter |
| `flash(color, dur)` | Overlay rect with alpha fade |
| Camera functions | `ctx.translate` + `ctx.scale` + `ctx.rotate` transforms |

---

## 16. Language Aliases

Td# provides English-like aliases for keywords to improve readability:

| Primary Keyword | Aliases |
|----------------|---------|
| `class` | `blueprint` |
| `extends` | `copies` |
| `return` | `send_back` |
| `while` | `as_long_as` |
| `for` | `repeat` |
| `break` | `stop` |
| `continue` | `skip` |

---

## 17. Error Handling

Errors are thrown as runtime exceptions with a message and line number:

```
RuntimeException: Division by zero (line 42)
RuntimeException: Undefined variable 'foobar' (line 13)
```

**Built-in function validation:** All built-in functions validate their argument count and types. Wrong usage produces descriptive errors:

```
RuntimeException: say() requires at least 1 argument (line 7)
RuntimeException: clamp() requires 3 arguments (value, min, max) (line 15)
```

**Execution limits:**
- For loops are capped at 100 million iterations to prevent infinite loops.
- The interpreter runs synchronously per frame in game mode.

---

## 18. Examples

### Hello World

```
function start()
    say("Hello, World!")
end
```

### Calculator (Console Mode)

```
say("=== Calculator ===")
var a = 10
var b = 3
say("a + b = " .. a + b)
say("a * b = " .. a * b)
say("a / b = " .. a / b)
```

### Simple Player Movement

```
var playerX = 320
var playerY = 240

function tick(dt)
    clear(#1a1a2e)

    if holding("right") then playerX += 200 * dt end
    if holding("left") then playerX -= 200 * dt end
    if holding("up") then playerY -= 200 * dt end
    if holding("down") then playerY += 200 * dt end

    circle(playerX, playerY, 20, Color(100, 200, 255))
    show_fps(true)
end
```

### Class Example

```
class Entity
    function constructor(x, y)
        this.x = x
        this.y = y
    end

    function dist(other)
        return dist(this.x, this.y, other.x, other.y)
    end
end

class Player copies Entity
    function constructor(x, y, name)
        // Call parent (not yet supported — must set manually)
        this.x = x
        this.y = y
        this.name = name
        this.speed = 200
    end

    function tick(dt)
        if holding("right") then this.x += this.speed * dt end
        if holding("left") then this.x -= this.speed * dt end
    end
end

var p = Player(320, 240, "Hero")
```

### Tilemap

```
function start()
    var map = grid(10, 10)
    fill_rect_map(map, 2, 2, 4, 4, 1)
    camera_bounds(Rect(0, 0, 320, 240))
end

function tick(dt)
    clear(#222034)
    tilemap(map, 0, 0)
end
```

### Full Game Template

```
var score = 0
var player = nil

function start()
    player = { x = 320, y = 440 }
    score = 0
    say("Game started!")
end

function tick(dt)
    clear(#1a1a2e)

    // Draw player
    rect(player.x - 15, player.y - 15, 30, 30, Color(100, 200, 255))

    // Movement
    if holding("left") then player.x -= 200 * dt end
    if holding("right") then player.x += 200 * dt end

    // Score display
    show("Score: " .. score, 10, 10, 20, Color(255, 255, 255))
end

function pressed(key)
    if key == "space" then
        score += 1
    end
end
```

---

> **End of Td# Language Bible v1.0**
