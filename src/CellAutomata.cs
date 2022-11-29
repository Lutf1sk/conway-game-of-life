
public class CellAutomata
{
	private int _hCells, _vCells;
	private int _cellCount;
	private int _boardIdx;
	private bool[][] _boards;

	public bool[] Board { get { return _boards[_boardIdx]; } }
	public int Width { get { return _hCells; } }
	public int Height { get { return _vCells; } }

	public CellAutomata(int hCells, int vCells)
	{
		_hCells = hCells;
		_vCells = vCells;
		_cellCount = hCells * vCells;
		_boardIdx = 0;
		_boards = new bool[][] { new bool[_cellCount], new bool[_cellCount] };
	}

	public void Clear()
	{
		for (int i = 0; i < _cellCount; ++i)
			_boards[_boardIdx][i] = false;
	}

	public void Randomize()
	{
		var rand = new Random();
		for (int i = 0; i < _cellCount; ++i)
			_boards[_boardIdx][i] = rand.Next(3) == 0;
	}

	public void Tick()
	{
		int nextBoardIdx = (_boardIdx + 1) % _boards.Length;
		for (int i = 0; i < _cellCount; ++i) {
			int neighbours = 0;
			int x = i % _hCells;
			int y = i / _hCells;

			if (x > 0 && _boards[_boardIdx][i - 1])
				++neighbours;
			if (x < _hCells-1 && _boards[_boardIdx][i + 1])
				++neighbours;
			if (y > 0 && _boards[_boardIdx][i - _hCells])
				++neighbours;
			if (y < _vCells-1 && _boards[_boardIdx][i + _hCells])
				++neighbours;

			if (x > 0 && y > 0 && _boards[_boardIdx][i - _hCells - 1])
				++neighbours;
			if (x < _hCells-1 && y > 0 && _boards[_boardIdx][i - _hCells + 1])
				++neighbours;
			if (x > 0 && y < _vCells-1 && _boards[_boardIdx][i + _hCells - 1])
				++neighbours;
			if (x < _hCells-1 && y < _vCells-1 && _boards[_boardIdx][i + _hCells + 1])
				++neighbours;

			if (_boards[_boardIdx][i])
				_boards[nextBoardIdx][i] = neighbours == 2 || neighbours == 3;
			else
				_boards[nextBoardIdx][i] = neighbours == 3;
		}
		_boardIdx = nextBoardIdx;
	}

	public int IndexFromCoords(int x, int y)
	{
		return y * _hCells + x;
	}
}

