using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace InstanceUtils.Utils.CommandUtils
{
    public class SnakeGame
    {
        private enum Direction { Up, Down, Left, Right }
        private record struct Point(int X, int Y);

        private readonly int _width;
        private readonly int _height;
        private readonly int _initialDelay;
        private readonly Random _rng = new Random();

        private bool _paused;
        private bool _gameOver;
        private int _score;
        private int _delay;
        private Direction _direction;
        private Direction _pendingDirection;
        private LinkedList<Point> _snake;
        private Point _food;

        public SnakeGame(int width = 30, int height = 15, int initialDelay = 150)
        {
            _width = width;
            _height = height;
            _initialDelay = initialDelay;
        }

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            while (true)
            {
                StartGame();
                if (!AskRestart())
                    break;
            }

            AnsiConsole.MarkupLine("\n[grey]Thanks for playing![/]");
            Console.CursorVisible = true;
        }

        private void StartGame()
        {
            InitializeGame();
            RenderStaticUI();

            while (!_gameOver)
            {
                HandleInput();
                if (!_paused)
                {
                    StepGame();
                    RenderDynamicElements();
                }
                Thread.Sleep(_delay);
            }

            RenderGameOver();
        }

        private void InitializeGame()
        {
            _score = 0;
            _delay = _initialDelay;
            _gameOver = false;
            _paused = false;

            _snake = new LinkedList<Point>();
            _snake.AddFirst(new Point(_width / 2, _height / 2));
            _snake.AddLast(new Point(_width / 2 - 1, _height / 2));
            _snake.AddLast(new Point(_width / 2 - 2, _height / 2));

            _direction = Direction.Right;
            _pendingDirection = _direction;
            _food = SpawnFood();

            Console.Clear();
        }

        private void HandleInput()
        {
            if (!Console.KeyAvailable)
                return;

            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.UpArrow: _pendingDirection = _direction == Direction.Down ? _direction : Direction.Up; break;
                case ConsoleKey.DownArrow: _pendingDirection = _direction == Direction.Up ? _direction : Direction.Down; break;
                case ConsoleKey.LeftArrow: _pendingDirection = _direction == Direction.Right ? _direction : Direction.Left; break;
                case ConsoleKey.RightArrow: _pendingDirection = _direction == Direction.Left ? _direction : Direction.Right; break;
                case ConsoleKey.P: _paused = !_paused; break;
                case ConsoleKey.Q: Environment.Exit(0); break;
            }
        }

        private void StepGame()
        {
            _direction = _pendingDirection;
            var head = _snake.First.Value;
            Point next = _direction switch
            {
                Direction.Up => new Point(head.X, head.Y - 1),
                Direction.Down => new Point(head.X, head.Y + 1),
                Direction.Left => new Point(head.X - 1, head.Y),
                Direction.Right => new Point(head.X + 1, head.Y),
                _ => head
            };

            // Collision check
            // === Border + Self Collision Check ===
            // Assuming border drawn at outer edges, valid space = (1, 1) to (_width - 2, _height - 2)
            if (next.X < 0 || next.Y < 0 || next.X > _width - 1 || next.Y > _height - 1 || SnakeContains(next))
            {
                _gameOver = true;
                return;
            }

            _snake.AddFirst(next);

            if (next.Equals(_food))
            {
                _score++;
                _food = SpawnFood();
                _delay = Math.Max(50, _delay - 5);
                DrawFood(_food);
                DrawScore();
            }
            else
            {
                var tail = _snake.Last.Value;
                _snake.RemoveLast();
                ErasePixel(tail);
            }

            DrawHead(next);
        }

        private bool SnakeContains(Point p)
        {
            foreach (var s in _snake)
                if (s.Equals(p))
                    return true;
            return false;
        }

        private Point SpawnFood()
        {
            while (true)
            {
                var p = new Point(_rng.Next(0, _width), _rng.Next(0, _height));
                if (!SnakeContains(p)) return p;
            }
        }

        private const int OffsetX = 1;  // leave 1 column for left border
        private const int OffsetY = 4;  // leave 3 lines for title + 1 border
        private void RenderStaticUI()
        {
            // Header
            AnsiConsole.MarkupLine("[bold green]🐍 Snake Game[/]   [grey](P: Pause, Q: Quit)[/]");
            AnsiConsole.MarkupLine("[grey]Use arrow keys to move[/]");
            AnsiConsole.MarkupLine($"[yellow]Score:[/] {_score}\n");

            int doubleWidth = _width * 2;

            // Draw top border
            Console.SetCursorPosition(OffsetX - 1, OffsetY - 1);
            AnsiConsole.Markup("[blue]" + new string('─', doubleWidth + 2) + "[/]");

            // Draw side borders
            for (int y = 0; y < _height; y++)
            {
                Console.SetCursorPosition(OffsetX - 1, OffsetY + y);
                AnsiConsole.Markup("[blue]│[/]");

                Console.SetCursorPosition(OffsetX + doubleWidth, OffsetY + y);
                AnsiConsole.Markup("[blue]│[/]");
            }

            // Draw bottom border
            Console.SetCursorPosition(OffsetX - 1, OffsetY + _height);
            AnsiConsole.Markup("[blue]" + new string('─', doubleWidth + 2) + "[/]");

            // Draw Food + Snake
            DrawFood(_food);
            foreach (var s in _snake)
                DrawHead(s);
        }

        private void RenderDynamicElements()
        {
            if (_paused)
            {
                Console.SetCursorPosition(_width / 2 - 3, _height / 2 + 5);
                AnsiConsole.Markup("[yellow]PAUSED[/]");
            }
            else
            {
                Console.SetCursorPosition(_width / 2 - 3, _height / 2 + 5);
                Console.Write("      "); // clear pause text
            }
        }

        private void DrawHead(Point p)
        {
            Console.SetCursorPosition(p.X * 2 + OffsetX, p.Y + OffsetY);
            AnsiConsole.Markup("[green]██[/]");
        }

        private void DrawFood(Point p)
        {
            Console.SetCursorPosition(p.X * 2 + OffsetX, p.Y + OffsetY);
            AnsiConsole.Markup("[red]██[/]");
        }

        private void ErasePixel(Point p)
        {
            Console.SetCursorPosition(p.X * 2 + OffsetX, p.Y + OffsetY);
            Console.Write("  ");
        }

        private void DrawScore()
        {
            Console.SetCursorPosition(8, 2);
            AnsiConsole.Markup($"[yellow]{_score}   [/]");
        }

        private void RenderGameOver()
        {
            Console.SetCursorPosition(_width / 2 - 5, _height / 2 + 6);
            AnsiConsole.Markup("[red bold]GAME OVER[/]");
        }

        private bool AskRestart()
        {
            Console.SetCursorPosition(0, _height + 8);
            AnsiConsole.Markup("\nPress [green]R[/] to restart or [red]Q[/] to quit...");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.R) return true;
                if (key == ConsoleKey.Q) return false;
            }
        }
    }

}
