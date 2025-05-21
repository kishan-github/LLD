#include <iostream>
#include <string>
#include <unordered_map>
#include <cstdlib>
#include <ctime> 

using namespace std;

/*
	Snake and Ladder game.
*/

class Player {
private:
	string Name;
	int Position;

public:
	Player(const string& name) : Name(name), Position(1) {}

	void setPosition(int newPos) {
		Position = newPos;
	}

	int getPosition() {
		return Position;
	}

	string getName() {
		return Name;
	}
};

class Board {
private:
	int Size;
	unordered_map<int, int> Snake;
	unordered_map<int, int> Ladder;

public:
	Board(int size, unordered_map<int, int> snake, unordered_map<int, int> ladder) :
		Size(size), Snake(snake), Ladder(ladder)
	{ }

	int getBoardSize() {
		return Size;
	}

	void AddSnake(int from, int to) {
		if (from <= to)
			return;

		Snake.insert({ from, to });
	}

	void AddLadder(int from, int to) {
		if (from >= to)
			return;

		Ladder.insert({ from, to });
	}

	int getNextPosition(int position) {
		if (Snake.find(position) != Snake.end())
			return Snake[position];
		if (Ladder.find(position) != Ladder.end())
			return Ladder[position];

		return position;
	}
};

class Dice {
private:
	int NumberOfFaces;

public:
	Dice(int numberOfFaces) :
		NumberOfFaces(numberOfFaces)
	{
		srand(time(nullptr));
	}

	int Roll() {
		return 1 + rand() % (NumberOfFaces);
	}

};

class Game {
private:
	Board GameBoard;
	vector<Player> Players;
	Dice GameDice;
	bool IsGameFinished;

public:
	Game(Board board, vector<Player> players, Dice dice) :
		GameBoard(board), Players(players), GameDice(dice), IsGameFinished(false)
	{ }

	Player Play() {
		int currPlayer = 3;
		while (!IsGameFinished) {
			currPlayer = (currPlayer + 1) % Players.size();
			int diceValue = GameDice.Roll();
			int newPos = Players[currPlayer].getPosition() + diceValue;

			cout << "Current Player : " << Players[currPlayer].getName() << ", Current Position : " << Players[currPlayer].getPosition();

			if (newPos > GameBoard.getBoardSize()) {
				cout << ", Oversteped.\n";
				continue;
			}

			newPos = GameBoard.getNextPosition(newPos);
			cout << ", New Position : " << newPos << "\n";
			if (newPos == GameBoard.getBoardSize()) {
				IsGameFinished = true;
				continue;
			}

			Players[currPlayer].setPosition(newPos);
		}

		return Players[currPlayer];
	}
};

int main() {
	Dice dice(6);
	unordered_map<int, int> ladder = { { 5, 20 }, { 31, 76 } };
	unordered_map<int, int> snake = { { 99, 9 }, { 62, 39 } };

	Board board(100, snake, ladder);

	vector<Player> players;
	players.push_back(Player("kishan"));
	players.push_back(Player("dheeraj"));
	players.push_back(Player("abcd"));
	players.push_back(Player("abcd1"));

	Game game(board, players, dice);
	Player winner = game.Play();

	cout << winner.getName() << " won the game!!!\n";

	return 0;
}