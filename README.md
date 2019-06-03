# Genetic Pathfinding

Genetic Pathfinding is project that shows sample genetic algorithm using SFML for graphics and no other library

## Installation

To use Genetic Pathfinding on your own machine just clone the repo and try one of sample project

```
git clone https://github.com/Tmacioso/GeneticPathfinding.git
```

## Usage

```cs
//At first create basic window
RenderWindow window = new RenderWindow(new VideoMode(750, 750), "Genetic Pathfinding");

//Then create target
var target = new RectangleShape(new Vector2f(30, 30));
target.Position = new Vector2f(0, 0);
target.FillColor = Color.Red;

//Create generation with 100 species, 3 best, 8000 steps, start in left down corner, 5% mutation chance, our previusly created target and window, and with random path on start
var generation = new Generation(100, 3, 8000, new Vector2f(window.Size.X - 1, 0), 5f, target, window, true);

//If window is open object will generate new populations
while (window.IsOpen)
    generation.NextGeneration();
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
