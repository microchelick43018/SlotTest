using System;
using System.Collections.Generic;

namespace SlotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Slot slot = new Slot();
            Console.WriteLine("1 - Full Cycle");
            Console.WriteLine("2 - MonteCarlo");
            int choice = Convert.ToInt32(Console.ReadLine());
            if (choice == 1)
            {
                slot.TestFullCycle();
            }
            else
            {
                slot.TestMonteCarlo();
            }
        }
    }

    class Slot
    {
        private const int SlotWidth = 5;
        private const int SlotHeight = 3;
        private int[] _ribbonsPositions = new int[SlotWidth];
        private List<List<int>> _ribbons = new List<List<int>>{
            new List<int>{ 10, 1, 4, 5, 8, 11, 4, 5, 9, 8, 7, 11, 10, 6, 7, 9, 8, 10, 3, 9, 11, 6, 9},
            new List<int>{ 6, 1, 3, 8, 7, 6, 9, 11, 2, 8, 4, 5, 10, 11, 4, 10, 8, 7, 9, 11, 5, 10, 9},
            new List<int>{ 6, 1, 3, 8, 7, 6, 9, 11, 2, 8, 4, 5, 10, 11, 4, 10, 8, 7, 9, 11, 5, 10, 9},
            new List<int>{ 6, 1, 3, 8, 7, 6, 9, 11, 2, 8, 4, 5, 10, 11, 4, 10, 8, 7, 9, 11, 5, 10, 9},
            new List<int>{ 6, 1, 3, 8, 7, 6, 9, 11, 2, 8, 4, 5, 10, 11, 4, 10, 8, 7, 9, 11, 5, 10, 9}
        };
        private Dictionary<int, double[]> _payTable = new Dictionary<int, double[]>()
        {
            {3, new double[]{ 2, 10, 20 } },
            {4, new double[]{ 1, 5, 10 } },
            {5, new double[]{ 1, 4, 6 } },
            {6, new double[]{ 1, 2, 5 } },
            {7, new double[]{ 0.5f, 1, 4 } },
            {8, new double[]{ 0.5f, 1, 4 } },
            {9, new double[]{ 0.1f, 0.5f, 2 } },
            {10, new double[]{ 0.1f, 0.5f, 2 } },
            {11, new double[]{ 0.1f, 0.5f, 2 } },
        };
        private int[,] _matrix = new int[SlotHeight, SlotWidth];
        private Random _random = new Random();
        private int[] _mas = new int[5];
        private Dictionary<int, double[]> _hitFrequency = new Dictionary<int, double[]>
        {
            { 3, new double[]{0, 0, 0} },
            { 4, new double[]{0, 0, 0} },
            { 5, new double[]{0, 0, 0} },
            { 6, new double[]{0, 0, 0} },
            { 7, new double[]{0, 0, 0} },
            { 8, new double[]{0, 0, 0} },
            { 9, new double[]{0, 0, 0} },
            { 10, new double[]{0, 0, 0} },
            { 11, new double[]{0, 0, 0} }
        };
        private Dictionary<int, int> _bonusGames = new Dictionary<int, int>
        {
            { 6, 0 },
            { 8, 0 },
            { 10, 0 }
        };
        private Dictionary<int, int> _bonusRetriggers = new Dictionary<int, int>
        {
            { 6, 0 },
            { 8, 0 },
            { 10, 0 }
        };
        private double _bonusRTP;
        private Dictionary<double, int> _baseWins = new Dictionary<double, int>();
        private Dictionary<double, int> _bonusWins = new Dictionary<double, int>();
        private double _averageFreespinsWithRetrigger;
        private double _iterationsCount = 100000000;
        private double _RTP;
        private int _amountBonusSpins = 0;
        private bool _isInBonusGame = false;

        public void TestMonteCarlo()
        {
            double amountWinX = 0f;
            double winX;
            int bonusGamesCount;
            for (int i = 0; i < _iterationsCount; i++)
            {
                GenerateRibbonsPositionsMonteCarlo();
                FillMatrix();
                winX = CountWinX();
                if (_baseWins.TryGetValue(winX, out int value2) == true)
                {
                    _baseWins[winX]++;
                }
                else
                {
                    _baseWins.Add(winX, 1);
                }
                bonusGamesCount = CheckForABonusTrigger();               
                if (bonusGamesCount > 5)
                {
                    _isInBonusGame = true;
                    StartBonusGame(bonusGamesCount);
                    _isInBonusGame = false;
                }
                amountWinX += winX;
            }
            foreach (var item in _hitFrequency)
            {
                for (int i = 0; i < 3; i++)
                {
                    item.Value[i] /= _iterationsCount;
                }
            }
            ShowBonusGamesInfoMonteCarlo();
            ShowHitFrequencyTable();
            foreach (var item in _baseWins)
            {
                _RTP += item.Value * item.Key;
            }
            _RTP /= _iterationsCount;
            Console.WriteLine($"RTP основной игры = {_RTP}");
            Console.WriteLine($"RTP бонусной игры: {_bonusRTP}");
            _RTP += _bonusRTP * _averageFreespinsWithRetrigger;
            Console.WriteLine($"RTP всей игры = {_RTP}");
            ShowDispersionMonteCarlo();
        }

        public void TestFullCycle()
        {
            CountAmountSlotPositions();
            double amountWinX = 0f;
            double winX;
            for (_mas[0] = 22; _mas[0] != -1; _mas[0]--)
            {
                for (_mas[1] = 22; _mas[1] != -1; _mas[1]--)
                {
                    for (_mas[2] = 22; _mas[2] != -1; _mas[2]--)
                    {
                        for (_mas[3] = 22; _mas[3] != -1; _mas[3]--)
                        {
                            for (_mas[4] = 22; _mas[4] != -1; _mas[4]--)
                            {
                                GenerateRibbonsPositionsFullCycle();
                                FillMatrix();
                                CheckForABonusTrigger();
                                winX = CountWinX();
                                if (_baseWins.TryGetValue(winX, out int value2) == true)
                                {
                                    _baseWins[winX]++;
                                }
                                else
                                {
                                    _baseWins.Add(winX, 1);
                                }
                                amountWinX += winX;
                            }
                        }
                    }
                }
            }
            foreach (var item in _hitFrequency)
            {
                for (int i = 0; i < 3; i++)
                {
                    item.Value[i] /= _iterationsCount;
                }
            }
            ShowBonusGamesInfoFullCycle();
            ShowHitFrequencyTable();
            _RTP = amountWinX / _iterationsCount;
            Console.WriteLine($"RTP основной игры = {_RTP}");
            _RTP *= 1 + _averageFreespinsWithRetrigger;
            Console.WriteLine($"RTP всей игры = {_RTP}");
            ShowDispersionFullCycle();
        }
        
        private void ShowDispersionMonteCarlo()
        {
            double baseDispersion = 0f;
            double bonusDisperion = 0f;
            _baseWins.Remove(0);
            _bonusWins.Remove(0);
            foreach (var item in _baseWins)
            {
                baseDispersion += (item.Key - _RTP) * (item.Key - _RTP) * item.Value;
            }
            baseDispersion /= _iterationsCount;
            Console.WriteLine($"Дисперсия базовой игры: {baseDispersion}");
            foreach (var item in _bonusWins)
            {
                bonusDisperion += (item.Key - _RTP) * (item.Key - _RTP) * item.Value;
            }
            bonusDisperion /= _amountBonusSpins;
            Console.WriteLine($"Дисперсия бонусной игры: {bonusDisperion}");
            Console.WriteLine($"Общая дисперсия: {baseDispersion + bonusDisperion * _averageFreespinsWithRetrigger}");
        }

        private void ShowDispersionFullCycle()
        {
            double baseDispersion = 0f;
            double bonusDisperion = 0f;
            _baseWins.Remove(0);
            _bonusWins.Remove(0);
            foreach (var item in _baseWins)
            {
                baseDispersion += (item.Key - _RTP) * (item.Key - _RTP) * item.Value;
            }
            baseDispersion /= _iterationsCount;
            Console.WriteLine($"Дисперсия базовой игры: {baseDispersion}");
            bonusDisperion = baseDispersion;// барабаны не отличаются
            Console.WriteLine($"Дисперсия бонусной игры: {bonusDisperion}");
            Console.WriteLine($"Общая дисперсия: {baseDispersion + bonusDisperion * _averageFreespinsWithRetrigger}");
        }

        private void StartBonusGame(int freeSpinsCount)
        {
            int bonusGamesCount;
            double winX;
            for (int i = 0; i < freeSpinsCount; i++)
            {
                GenerateRibbonsPositionsMonteCarlo();
                FillMatrix();
                bonusGamesCount = CheckForABonusTrigger();
                winX = CountWinX();
                if (_bonusWins.TryGetValue(winX, out int value1))
                {
                    _bonusWins[winX]++;
                }
                else
                {
                    _bonusWins.Add(winX, 1);
                }
                if (bonusGamesCount > 5)
                {
                    _bonusRetriggers[bonusGamesCount]++;
                    StartBonusGame(bonusGamesCount);
                }        
                _bonusRTP += winX;
            }
        }

        private void CountAmountSlotPositions()
        {
            _iterationsCount = 1;
            foreach (var ribbon in _ribbons)
            {
                _iterationsCount *= ribbon.Count;
            }
        }

        private void ShowHitFrequencyTable()
        {
            Console.WriteLine();
            Console.WriteLine("Таблица вероятностей выпадения каждой комбинации");
            for (int k = 3; k <= 5; k++)
            {
                foreach (var item in _hitFrequency)
                {
                    Console.Write($"{k}x - {item.Key}\t");
                    Console.WriteLine($"{item.Value[k - 3]}");
                    Console.WriteLine();
                }
            }

        }

        private int CheckForABonusTrigger()
        {
            int scattersCount = 0;
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    if (_matrix[i, j] == 1)
                    {
                        scattersCount++;
                    }
                }
            }
            if (scattersCount > 2)
            {
                _bonusGames[scattersCount * 2]++;
                return scattersCount * 2;
            }
            return 0;
        }

        private void ShowBonusGamesInfoFullCycle()
        {
            int amountBonusSpins = 0;
            int amountBonusTriggers = 0;
            double averageFreespinsWithoutRetrigger;
            foreach (var item in _bonusGames)
            {
                amountBonusSpins += item.Key * item.Value;
                amountBonusTriggers += item.Value;
            }
            Console.WriteLine($"Вероятность фриспинов за спин = {amountBonusTriggers / _iterationsCount}");
            averageFreespinsWithoutRetrigger = amountBonusSpins / _iterationsCount;
            Console.WriteLine($"Среднее количество фриспинов за 1 спин без ретриггера = {averageFreespinsWithoutRetrigger}");
            _averageFreespinsWithRetrigger = amountBonusSpins / _iterationsCount;
            _averageFreespinsWithRetrigger = averageFreespinsWithoutRetrigger;
            double temp = averageFreespinsWithoutRetrigger;
            for (int i = 0; i < 1000; i++)
            {
                temp *= averageFreespinsWithoutRetrigger;
                _averageFreespinsWithRetrigger += temp;
            }
            Console.WriteLine($"С ретриггером: {_averageFreespinsWithRetrigger}");
        }

        private void ShowBonusGamesInfoMonteCarlo()
        {
            int amountBonusTriggers = 0;
            int amountBonusSpinsRetriggers = 0;
            foreach (var item in _bonusGames)
            {
                _amountBonusSpins += item.Key * item.Value;
                amountBonusTriggers += item.Value;
            }
            Console.WriteLine($"Вероятность фриспинов за спин = {amountBonusTriggers / _iterationsCount}");
            int amountRetriggerSpins = 0;
            foreach (var item in _bonusRetriggers)
            {
                amountRetriggerSpins += item.Key * item.Value;
            }
            Console.WriteLine($"Среднее количество фриспинов за 1 спин без ретриггера = {(_amountBonusSpins-amountRetriggerSpins) / _iterationsCount}");
            _bonusRTP /= _amountBonusSpins;
            _averageFreespinsWithRetrigger = (_amountBonusSpins) / _iterationsCount;
            Console.WriteLine($"С ретриггером: {_averageFreespinsWithRetrigger}");
        }

        private double CountWinX()
        {
            double winX = 0;
            int multiple = 0;
            int[] firstValues = GetFirstValues();
            int paylinesCount = 1;
            int similuarValuesCount = 0;
            for (int i1 = 0; i1 < firstValues.Length; i1++)
            {
                paylinesCount = 1;
                multiple = 1;
                for (int i = 1; i < SlotWidth; i++)
                {
                    similuarValuesCount = 0;
                    for (int j = 0; j < SlotHeight; j++)
                    {
                        if (_matrix[j, i] == firstValues[i1] || _matrix[j, i] == 2)
                        {
                            similuarValuesCount++;
                            if (similuarValuesCount == 1)
                                multiple++;
                        }
                    }
                    if (similuarValuesCount == 0)
                    {
                        break;
                    }
                    else
                    {
                        paylinesCount *= similuarValuesCount;
                    }
                }
                if (multiple > 2 && firstValues[i1] > 2 && _isInBonusGame == false)
                    _hitFrequency[firstValues[i1]][multiple - 3] += paylinesCount;
                winX += GetWinXFromPayTable(firstValues[i1], multiple) * paylinesCount;
            }
            return winX;
        }

        private int[] GetFirstValues()
        {
            int[] firstValues = new int[SlotHeight];
            for (int i = 0; i < SlotHeight; i++)
            {
                firstValues[i] = _matrix[i, 0];
            }
            return firstValues;
        }

        private double GetWinXFromPayTable(int value, int multiple)
        {
            if (multiple > 2 && value > 2)
                return _payTable[value][multiple - 3];
            else
                return 0;
        }

        private void ShowMatrix()
        {
            for (int i = 0; i < SlotHeight; i++)
            {
                for (int j = 0; j < SlotWidth; j++)
                {
                    Console.Write($"{_matrix[i, j]}\t");
                }
                Console.WriteLine();
            }
        }

        private void FillMatrix()
        {
            for (int i = 0; i < SlotWidth; i++)
            {
                for (int j = 0; j < SlotHeight; j++)
                {
                    _matrix[j, i] = _ribbons[i][(_ribbonsPositions[i] + j) % _ribbons[i].Count];
                }
            }
        }

        private void GenerateRibbonsPositionsMonteCarlo()
        {
            for (int i = 0; i < SlotWidth; i++)
            {
                _ribbonsPositions[i] = _random.Next(0, _ribbons[i].Count);
            }
        }

        private void GenerateRibbonsPositionsFullCycle()
        {
            for (int i = 0; i < SlotWidth; i++)
            {
                _ribbonsPositions[i] = _mas[i];
            }
        }
    }
}
