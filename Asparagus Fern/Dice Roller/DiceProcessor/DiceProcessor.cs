using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Asparagus_Fern.Dice_Roller.DiceProcessor
{
    class DiceProcessor
    {
        static readonly Regex whiteSpaceRegex = new Regex(@"\s+");

        public enum DieType
        {
            Sum,
            Disadvantage,
            Advantage,
        }

        bool isValid = false;
        DieType dieType = DieType.Sum;
        int dieCount = 1;
        int dieSize = 0;
        int modifier = 0;

        public DiceProcessor(string dieText)
        {
            ProcessDieText(dieText);
        }

        public void ProcessDieText(string dieText)
        {
            string trimmedText = whiteSpaceRegex.Replace(dieText, "");
            if (trimmedText.Length == 0)
            {
                return;
            }

            string[] dieParts = trimmedText.Split('d', 'D');
            if (dieParts.Length != 2)
            {
                return;
            }

            if (dieParts[0].Length == 0 || !int.TryParse(dieParts[0], out dieCount) || dieCount <= 0)
            {
                dieCount = 1;
            }

            if (dieParts[1].Length == 0)
            {
                return;
            }

            if (dieCount > 10000)
            {
                return;
            }

            if (dieParts[1].Contains("+"))
            {
                string[] modiferParts = dieParts[1].Split('+');
                if (modiferParts.Length != 2)
                {
                    return;
                }

                ProcessDieSize(modiferParts[0]);
                ProcessModifer(modiferParts[1], 1);
            }
            else if (dieParts[1].Contains('-'))
            {
                string[] modiferParts = dieParts[1].Split('-');
                if (modiferParts.Length != 2)
                {
                    return;
                }

                ProcessDieSize(modiferParts[0]);
                ProcessModifer(modiferParts[1], -1);
            }
            else
            {
                ProcessDieSize(dieParts[1]);
                modifier = 0;
            }

            if (dieText.Contains('w'))
            {
                dieType = DieType.Advantage;
            }
            else if (dieText.Contains('l'))
            {
                dieType = DieType.Disadvantage;
            }
        }

        public void ProcessModifer(string modifierText, int modMultiplier)
        {
            if (modifierText.Length == 0)
            {
                return;
            }

            int startIndex = 0;
            int finishIndex = 0;
            GetNextNumberParams(modifierText, 0, ref startIndex, ref finishIndex);

            if (int.TryParse(modifierText.Substring(startIndex, finishIndex - startIndex), out modifier))
            {
                modifier *= modMultiplier;
            }
            else
            {
                modifier = 0;
            }
        }

        public void ProcessDieSize(string modifierText)
        {
            int startIndex = 0;
            int finishIndex = 0;
            GetNextNumberParams(modifierText, 0, ref startIndex, ref finishIndex);

            if (int.TryParse(modifierText.Substring(startIndex, finishIndex - startIndex), out dieSize))
            {
                isValid = true;
            }
        }

        public void GetNextNumberParams(string text, int offset, ref int start, ref int finish)
        {
            start = -1;
            finish = -1;
            for (int i = offset; i < text.Length; i++)
            {
                if (start == -1)
                {
                    if (Char.IsDigit(text[i]) || text[i] == '-')
                    {
                        start = i;
                    }
                }
                else
                {
                    if (!Char.IsDigit(text[i]))
                    {
                        finish = i;
                        return;
                    }
                }
            }
            finish = text.Length;
        }

        public bool IsValid()
        {
            return isValid;
        }

        public DieRoll Roll()
        {
            DieRoll dieRoll = new DieRoll();
            for (int i = 0; i < dieCount; i++)
            {
                dieRoll.Rolls.Add(new Random().Next(1, dieSize + 1));
            }

            switch (dieType)
            {
                case DieType.Sum:
                    dieRoll.Result = dieRoll.Rolls.Sum() + modifier;
                    break;
                case DieType.Advantage:
                    dieRoll.Result = dieRoll.Rolls.Max() + modifier;
                    break;
                case DieType.Disadvantage:
                    dieRoll.Result = dieRoll.Rolls.Min() + modifier;
                    break;
            }

            return dieRoll;
        }

        public override string ToString()
        {
            char dieTypeChar = ' ';
            if (dieType == DieType.Advantage)
            {
                dieTypeChar = 'W';
            }
            else if (dieType == DieType.Disadvantage)
            {
                dieTypeChar = 'L';
            }
            
            if (modifier == 0)
            {
                return $"{dieCount}d{dieSize} {dieTypeChar}";
            }
            else if (modifier >= 0)
            {
                return $"{dieCount}d{dieSize}+{modifier} {dieTypeChar}";
            }
            return $"{dieCount}d{dieSize}{modifier} {dieTypeChar}";
        }
    }
}
