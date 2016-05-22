using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C
{
    internal sealed class SourceCode
    {
        private struct Block
        {
            public int ExternPosition;
            public int InnerPosition;
            public int Lenght;
            public string Source;
        }

        private Block[] _blocks;
        private int _blockIndex;

        public int Length { get; private set; }

        public char this[int index]
        {
            get
            {
                return getChar(index);
            }
        }

        public SourceCode()
        {
            _blocks = new Block[0];
        }

        public SourceCode(string text)
        {
            _blocks = new Block[1]
            {
                new Block
                {
                    ExternPosition = 0,
                    InnerPosition = 0,
                    Lenght = text.Length,
                    Source = text
                }
            };

            Length = text.Length;
        }

        private int getBlockIndex(int position)
        {
            bool restart = _blockIndex != 0;
            var delta = _blocks.Length / 2 + _blocks.Length % 2;

            for (;;)
            {
                if (_blocks[_blockIndex].ExternPosition <= position
                    && _blocks[_blockIndex].ExternPosition + _blocks[_blockIndex].Lenght > position)
                {
                    return _blockIndex;
                }
                else if (restart)
                {
                    _blockIndex = 0;
                    continue;
                }

                if (_blocks[_blockIndex].ExternPosition + _blocks[_blockIndex].Lenght <= position)
                {
                    _blockIndex += delta;
                }

                if (_blocks[_blockIndex].ExternPosition >= position)
                {
                    _blockIndex -= delta;
                }

                if (delta == 1)
                    throw new IndexOutOfRangeException();

                delta = delta / 2 + delta % 2;
            }
        }

        private char getChar(int index)
        {
            var blockIndex = getBlockIndex(index);
            return _blocks[blockIndex].Source[index - _blocks[blockIndex].ExternPosition + _blocks[blockIndex].InnerPosition];
        }

        public void ReplaceToken(string pattern, string s)
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            for (var i = 0; i < this.Length; i++)
            {
                if (this[i] == pattern[0])
                {
                    var j = 1;
                    for (; j < pattern.Length && j < this.Length; j++)
                    {
                        if (this[i + j] != pattern[j])
                            break;
                    }

                    if (j == pattern.Length)
                    {
                        if ((i + pattern.Length >= this.Length || Parser.isIdentificatorTerminator(this[i - 1]))
                            && (i == 0 || Parser.isIdentificatorTerminator(this[i - 1])))
                        {
                            var blockIndex = getBlockIndex(i);
                            var inblockPosition = i - _blocks[blockIndex].ExternPosition;

                            var moveDelta = 1;
                            var checkLen = pattern.Length - _blocks[blockIndex].Lenght + inblockPosition;

                            if (checkLen > 0)
                            {
                                moveDelta++;
                                do
                                {
                                    blockIndex++;
                                    inblockPosition = 0;
                                    checkLen = pattern.Length - _blocks[blockIndex].Lenght + inblockPosition;
                                    moveDelta--;
                                }
                                while (checkLen > 0);

                                blockIndex = getBlockIndex(i);
                            }

                            if (moveDelta > 0)
                            {
                                var emptyItemsCount = 0;
                                if (_blocks.Length > 1)
                                {
                                    while (_blocks[_blocks.Length - 1 - emptyItemsCount].ExternPosition == 0)
                                        emptyItemsCount++;
                                }

                                if (emptyItemsCount == 0)
                                {
                                    var newBlocks = new Block[Math.Max(3, _blocks.Length * 5 / 3)];
                                    for (var k = 0; k <= blockIndex; k++)
                                        newBlocks[k] = _blocks[k];
                                    // TODO
                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
