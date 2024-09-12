<div align="center">

 <img src="https://github.com/pietraldo/Chess-GUI/blob/main/obrazki/Zrzut%20ekranu%202024-09-12%20124421.png" width="60%" />

# Chess-GUI

Application for playing chess. You can play with friend or against any chess engine with UCI protocol.

</div>

## ⚡️ Quick start
This application was made in Visual Studio so you have to just download repository and open .sln file and compile. 

Then you can adjust settings in Settings.json

## ⚙️ Settings & Options
In file Settings.json you can set your options.

#### Example of `Settings.json`
```json
{
  "start_position": "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1",
  "disable_order": false,
  "whose_turn": "WHITE",
  "mute": false,
  "play_with_engine": false,
  "engine_path": "C:\\Users\\pietr\\Desktop\\chess_engine\\x64\\Debug\\chess_engine.exe",
  "engine_color": "BLACK",
  "engine_depth": 4
}
```
| Parametr name    | Description                                                                             | Values           |
| ---------------- | --------------------------------------------------------------------------------------- | ---------------- |
| start_position   | this is fen position which will be set after starting application                       | FEN Position     |
| disable_order    | if it is set to true it means for example that white can do two moves in the row        | true, false      |
| whose_turn       | means who start the game                                                                | WHITE, BLACK     |
| mute             | if set to true it turns of all sound of captures, moves and so on                       | true, false      |
| play_with_engine | if set to true you will be playing against engine. Be sure to give path to engine.      | true, false      |
| engine_path      | path to engine with uci protocol that you be playing against                            | path to exe file |
| engine_color     | color which engine will be playing, you will be playing oposit color                    | WHITE, BLACK     |
| engine_depth     | parametr that will be passed to engine. It tells to which depth engine will be counting | numer > 0        |

## 📖 Used Technology
In this project I used MonoGame framework for displaying and moving pieces and all was written with C#

## 🔥Screenshots from game
<p align="center">
  <img src="https://github.com/pietraldo/Chess-GUI/blob/main/obrazki/Zrzut%20ekranu%202024-09-12%20124421.png" width="45%" />
  <img src="https://github.com/pietraldo/Chess-GUI/blob/main/obrazki/Zrzut%20ekranu%202024-09-12%20151116.png" width="45%" />
</p>




Images of pieces from lichess.org
