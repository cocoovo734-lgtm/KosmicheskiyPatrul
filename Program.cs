using System;
using System.Collections.Generic;

namespace KosmicheskiyPatrul
{
    internal class Program
    {
        private const int FieldWidth = 5;
        private const int FieldHeight = 5;

        private static int playerX = 2;
        private static int playerY = 2;
        private static int health = 100;
        private static int score = 0;
        private static int collectedCores = 0;
        private static int turnsRemaining = 1000;

        private static bool isPlaying = true;
        private static bool hasWon = false;
        private static bool hasLost = false;
        private static bool enemyTriggered = false;
        private static bool medkitCollected = false;

        private static readonly HashSet<string> collectedCoresOnMap = new HashSet<string>();

        private static readonly (int x, int y)[] CoreLocations =
        {
            (0, 0),
            (4, 0),
            (4, 4)
        };

        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("==================================");
            Console.WriteLine("       КОСМИЧЕСКИЙ ПАТРУЛЬ");
            Console.WriteLine("==================================");
            Console.WriteLine("Соберите 3 энергетических ядра, чтобы спасти станцию.");
            Console.WriteLine("Команды: w/a/s/d — движение, map — карта, status — статус,");
            Console.WriteLine("         help — помощь, exit или q — выход.");
            Console.WriteLine();

            // Основной игровой цикл завершается при победе, поражении,
            // команде выхода или исчерпании защитного лимита ходов.
            while (isPlaying && !hasWon && !hasLost && turnsRemaining > 0)
            {
                DrawStatus();
                Console.Write("Команда: ");
                string command = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

                turnsRemaining--;

                // SWITCH используется для выбора одной команды из фиксированного набора.
                // Для отдельных постоянных значений switch нагляднее цепочки if.
                switch (command)
                {
                    case "w":
                        MovePlayer(0, -1);
                        break;
                    case "s":
                        MovePlayer(0, 1);
                        break;
                    case "a":
                        MovePlayer(-1, 0);
                        break;
                    case "d":
                        MovePlayer(1, 0);
                        break;
                    case "map":
                        DrawMap();
                        break;
                    case "status":
                        DrawStatus();
                        break;
                    case "help":
                        ShowHelp();
                        break;
                    case "fight":
                        FightEnemy();
                        break;
                    case "exit":
                    case "q":
                        isPlaying = false;
                        Console.WriteLine("Вы покидаете патруль.");
                        break;
                    default:
                        Console.WriteLine("Неизвестная команда. Введите help для списка команд.");
                        break;
                }

                if (isPlaying && !hasWon && !hasLost)
                {
                    CheckCurrentCell();
                    CheckGameState();
                }
            }

            if (turnsRemaining == 0 && isPlaying && !hasWon && !hasLost)
            {
                Console.WriteLine("Достигнут лимит ходов. Партия безопасно завершена.");
            }

            PrintFinalResult();
        }

        private static void MovePlayer(int deltaX, int deltaY)
        {
            int newX = playerX + deltaX;
            int newY = playerY + deltaY;

            // Каждая граница поля проверяется отдельным явным условием.
            if (newX < 0)
            {
                Console.WriteLine("Нельзя выйти за левую границу поля.");
                return;
            }

            if (newX >= FieldWidth)
            {
                Console.WriteLine("Нельзя выйти за правую границу поля.");
                return;
            }

            if (newY < 0)
            {
                Console.WriteLine("Нельзя выйти за верхнюю границу поля.");
                return;
            }

            if (newY >= FieldHeight)
            {
                Console.WriteLine("Нельзя выйти за нижнюю границу поля.");
                return;
            }

            playerX = newX;
            playerY = newY;
            Console.WriteLine($"Вы переместились в точку ({playerX}; {playerY}).");
        }

        private static void FightEnemy()
        {
            Console.WriteLine("Вы вступили в бой с пиратским дроном!");
            health -= 35;
            score -= 20;

            // Явная защита от отрицательных значений.
            if (health < 0)
            {
                health = 0;
            }

            if (score < 0)
            {
                score = 0;
            }
        }

        private static void CheckCurrentCell()
        {
            string currentPosition = $"{playerX},{playerY}";

            // Сценарий 1: подбор энергетического ядра.
            foreach (var core in CoreLocations)
            {
                string corePosition = $"{core.x},{core.y}";
                if (playerX == core.x && playerY == core.y &&
                    !collectedCoresOnMap.Contains(corePosition))
                {
                    collectedCoresOnMap.Add(corePosition);
                    collectedCores++;
                    score += 100;
                    Console.WriteLine("Получено энергетическое ядро! +100 очков.");
                }
            }

            // Сценарий 2: попадание в опасную зону с противником.
            if (playerX == 2 && playerY == 0 && !enemyTriggered)
            {
                enemyTriggered = true;
                health -= 35;
                score -= 20;

                // Явная защита от отрицательного здоровья и счёта.
                if (health < 0)
                {
                    health = 0;
                }

                if (score < 0)
                {
                    score = 0;
                }

                Console.WriteLine("Засада пиратского дрона! Получен урон: 35.");
                Console.WriteLine("За потерю ресурсов снято 20 очков.");
            }

            // Сценарий 3: подбор аптечки и лечение.
            if (playerX == 0 && playerY == 4 && !medkitCollected)
            {
                medkitCollected = true;
                int oldHealth = health;
                health += 30;

                if (health > 100)
                {
                    health = 100;
                }

                Console.WriteLine($"Найдена аптечка. Здоровье: {oldHealth} -> {health}.");
            }

            if (collectedCores == CoreLocations.Length)
            {
                hasWon = true;
                isPlaying = false;
                Console.WriteLine("Все энергетические ядра собраны!");
            }
        }

        private static void CheckGameState()
        {
            // IF / ELSE IF / ELSE проверяет диапазоны состояния здоровья.
            // Проверки идут от частного случая к общему.
            if (health <= 0)
            {
                hasLost = true;
                isPlaying = false;
                Console.WriteLine("Критическое состояние: здоровье закончилось.");
            }
            else if (health <= 30)
            {
                Console.WriteLine("ВНИМАНИЕ: мало здоровья! Найдите аптечку.");
            }
            else
            {
                Console.WriteLine("Состояние героя стабильное.");
            }
        }

        private static void DrawStatus()
        {
            Console.WriteLine();
            Console.WriteLine($"Позиция: ({playerX}; {playerY}) | Здоровье: {health}/100");
            Console.WriteLine($"Ядра: {collectedCores}/3 | Счёт: {score} | Осталось ходов: {turnsRemaining}");
        }

        private static void DrawMap()
        {
            Console.WriteLine("Карта: P — игрок, E — опасная зона, M — аптечка, * — ядро, . — пусто");
            for (int y = 0; y < FieldHeight; y++)
            {
                for (int x = 0; x < FieldWidth; x++)
                {
                    string position = $"{x},{y}";
                    char cell = '.';

                    if (playerX == x && playerY == y)
                    {
                        cell = 'P';
                    }
                    else if (x == 2 && y == 0 && !enemyTriggered)
                    {
                        cell = 'E';
                    }
                    else if (x == 0 && y == 4 && !medkitCollected)
                    {
                        cell = 'M';
                    }
                    else
                    {
                        foreach (var core in CoreLocations)
                        {
                            if (core.x == x && core.y == y &&
                                !collectedCoresOnMap.Contains(position))
                            {
                                cell = '*';
                            }
                        }
                    }

                    Console.Write(cell + " ");
                }
                Console.WriteLine();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("w — вверх, s — вниз, a — влево, d — вправо.");
            Console.WriteLine("map — показать карту; status — показать параметры.");
            Console.WriteLine("fight — вступить в бой (урон 35, потеря 20 очков).");
            Console.WriteLine("Соберите все 3 ядра для победы. Не допускайте потери всего здоровья.");
            Console.WriteLine("exit или q — завершить партию.");
        }

        private static void PrintFinalResult()
        {
            Console.WriteLine();
            Console.WriteLine("========== ИТОГ ПАРТИИ ==========");
            if (hasWon)
            {
                Console.WriteLine("РЕЗУЛЬТАТ: ПОБЕДА!");
            }
            else if (hasLost)
            {
                Console.WriteLine("РЕЗУЛЬТАТ: ПОРАЖЕНИЕ.");
            }
            else
            {
                Console.WriteLine("РЕЗУЛЬТАТ: ВЫХОД ИЗ ИГРЫ.");
            }

            Console.WriteLine($"Собрано ядер: {collectedCores}/3");
            Console.WriteLine($"Итоговое здоровье: {health}/100");
            Console.WriteLine($"Итоговый счёт: {score}");
            Console.WriteLine("Спасибо за игру!");
        }
    }
}
