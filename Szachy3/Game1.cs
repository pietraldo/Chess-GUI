using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace Szachy3
{
    public class Game1 : Game
    {
        // board
        Texture2D board;
        Vector2 board_position;
        int board_start_position_x = 0;
        int board_start_position_y = 0;
        int board_offset_x = 50;
        int board_offset_y = 50;

        // pieces
        Texture2D[] piece = new Texture2D[32];
        Vector2[] piece_position = new Vector2[32];
        public int index_dragged_piece = -1;
        public int prev_position_x;
        public int prev_position_y;
        public const int figure_size= 100;
        public static float[] curent_size = new float[32]; // potrzebne do animacji
        public static bool[] animation_on = new bool[32]; // potrzebne do animacji
        public static float[] elapsedTime=new float[32]; // potrzebne do animacji
        public static Vector2[] przemieszczenie= new Vector2[32];
        public static Vector2[] miejsce_docelowe= new Vector2[32];


        // game
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // sound
        Song move_sound;
        Song wrong_move_sound;
        Song capture_sound;

        // button
        Texture2D button1;
        Vector2 button1_position;
        int button1_start_position_x = 1000;
        int button1_start_position_y = 350;

        // animation
        static float initialScale = 1.0f;         // Initial scale
        static float targetScale = 0.3f;          // Target scale 
        static float animationDuration = 0.2f;    // Animation duration in seconds
        static float moveSpeed = 5000.0f;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1300; // Set your desired width
            _graphics.PreferredBackBufferHeight = 900; // Set your desired height

        }

        protected override void Initialize()
        {
            this.move_sound = Content.Load<Song>("tech_sound");
            this.wrong_move_sound = Content.Load<Song>("sound2");
            this.capture_sound = Content.Load<Song>("explo_sound");
            Initialize_Board();
            Initialize_Pieces();
            button1 = Content.Load<Texture2D>("resetuj");
            button1_position = new Vector2(button1_start_position_x, button1_start_position_y);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            MouseState mouseState = Mouse.GetState();

            // wyswietlanie kursora raczki nad figura
            bool over = false;
            for (int i = 0; i < 32; i++)
            {
                if (IsMouseOverObject(mouseState, i))
                {
                    Mouse.SetCursor(MouseCursor.Hand);
                    over = true;
                }
            }
            if (!over) Mouse.SetCursor(MouseCursor.Arrow);


            // lapanie figur
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if(index_dragged_piece==-1)
                {
                    for (int i = 0; i < 32; i++)
                    {
                        if (IsMouseOverObject(mouseState, i))
                        {
                            index_dragged_piece = i;
                            prev_position_x = (int)piece_position[i].X;
                            prev_position_y = (int)piece_position[i].Y;
                        }
                    }
                }
                else
                {
                    piece_position[index_dragged_piece] = new Vector2(mouseState.X-board_offset_x-figure_size/2, mouseState.Y-board_offset_y- figure_size / 2);
                }
            }
            else
            {
                if(index_dragged_piece!=-1) // Puszczenie pionka
                {
                    // dopasowanie pozycji pionka do pola
                    piece_position[index_dragged_piece] = new Vector2((int)((piece_position[index_dragged_piece].X+ figure_size / 2) / 100)*100, (int)((piece_position[index_dragged_piece].Y+ figure_size / 2) / 100)*100);
                    
                    
                    bool czy_jest_kolizja = false;
                    // Sprawdzanie kolizji
                    for(int i=0;i<32; i++)
                    {
                        if (i == index_dragged_piece) continue;

                        //zbicie
                        if (piece_position[index_dragged_piece].X == piece_position[i].X && piece_position[index_dragged_piece].Y == piece_position[i].Y)
                        {
                            czy_jest_kolizja = true;
                            if((int)(index_dragged_piece/16) ==(int)( i/16)) // czy nie zbija swojego
                            {
                                piece_position[index_dragged_piece].X = prev_position_x; // powrot na poprzednie miejsce
                                piece_position[index_dragged_piece].Y = prev_position_y;
                                MediaPlayer.Play(wrong_move_sound);
                            }
                            else
                            {
                                MediaPlayer.Play(capture_sound);
                                if (i < 16)
                                {
                                    miejsce_docelowe[i] = new Vector2(board.Width + board_offset_x + (i % 8) * 30, ((int)(i / 8) + 1) % 2 * 30 + 700);
                                }
                                else
                                {
                                    miejsce_docelowe[i] = new Vector2(board.Width + board_offset_x + (i % 8) * 30, ((int)((i - 16) / 8)) * 30);
                                }
                                animation_on[i] = true;
                                przemieszczenie[i] =miejsce_docelowe[i]- piece_position[i];
                                przemieszczenie[i].Normalize();
                                
                            }
                        }
                    }

                    if(!czy_jest_kolizja)
                        MediaPlayer.Play(move_sound);
                }
                    
                index_dragged_piece = -1;
            }

            // funkcja ktora zwraca czy myszka jest nad figura o indeksie i
            bool IsMouseOverObject(MouseState mouseState, int i)
            {
                Rectangle objectBounds = new Rectangle((int)piece_position[i].X+board_offset_x, (int)piece_position[i].Y+board_offset_y, piece[i].Width, piece[i].Height);
                return objectBounds.Contains(mouseState.X, mouseState.Y);
            }

            

            // klikanie przycisku
            if (mouseState.LeftButton == ButtonState.Pressed && (new Rectangle((int)button1_position.X, (int)button1_position.Y, button1.Width, button1.Height)).Contains(mouseState.X, mouseState.Y))
                Initialize_Pieces();


            // animacja zbicia
            float moveAmount = moveSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            for (int i = 0; i < 32; i++)
            {
                if (animation_on[i])
                {
                    elapsedTime[i] += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (elapsedTime[i] <= animationDuration)
                    {
                        float t = elapsedTime[i] / animationDuration;
                        curent_size[i] = MathHelper.Lerp(initialScale, targetScale, t);
                    }
                    else
                    {
                        // Animation complete, set the final scale
                        curent_size[i] = targetScale;
                    }
                    float distance = Vector2.Distance(piece_position[i], miejsce_docelowe[i]);
                    if (distance > moveAmount)
                    {
                        piece_position[i] += przemieszczenie[i] * moveAmount;
                    }
                    else
                    {
                        piece_position[i] = miejsce_docelowe[i]; // Docelowa pozycja osiągnięta
                    }
                }
            }
               
            
           


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Rysowanie elementów
            _spriteBatch.Begin();
            _spriteBatch.Draw(board, new Vector2(board_position.X+board_offset_x, board_position.Y+board_offset_y), Color.White);
            rysuj_czerowne_pola(new Vector2[] {new Vector2(100,400), new Vector2(700, 100), new Vector2(700, 700) });
            _spriteBatch.Draw(button1, button1_position, Color.White);
            

            for (int i=0; i<32; i++)
            {
                _spriteBatch.Draw(piece[i], new Vector2(piece_position[i].X + board_offset_x, piece_position[i].Y + board_offset_y), null, Color.White, 0f, Vector2.Zero, curent_size[i], SpriteEffects.None, 0f);
            }

            // rysowanie pionka na samej gorze
            if(index_dragged_piece!=-1)
                _spriteBatch.Draw(piece[index_dragged_piece], new Vector2(piece_position[index_dragged_piece].X + board_offset_x, piece_position[index_dragged_piece].Y + board_offset_y), null, new Color(255,255,255,128));

            _spriteBatch.End();

            base.Draw(gameTime);
        }


        protected void Initialize_Board()
        {
            board = Content.Load<Texture2D>("board");
            board_position = new Vector2(board_start_position_x, board_start_position_y);
        }
        protected void Initialize_Pieces()
        {
            piece[0] = Content.Load<Texture2D>("wR");
            piece[1] = Content.Load<Texture2D>("wN");
            piece[2] = Content.Load<Texture2D>("wB");
            piece[3] = Content.Load<Texture2D>("wQ");
            piece[4] = Content.Load<Texture2D>("wK");
            piece[5] = Content.Load<Texture2D>("wB");
            piece[6] = Content.Load<Texture2D>("wN");
            piece[7] = Content.Load<Texture2D>("wR");

            for (int i = 8; i < 16; i++)
            {
                piece[i] = Content.Load<Texture2D>("wP");
            }

            piece[16] = Content.Load<Texture2D>("bR");
            piece[17] = Content.Load<Texture2D>("bN");
            piece[18] = Content.Load<Texture2D>("bB");
            piece[19] = Content.Load<Texture2D>("bQ");
            piece[20] = Content.Load<Texture2D>("bK");
            piece[21] = Content.Load<Texture2D>("bB");
            piece[22] = Content.Load<Texture2D>("bN");
            piece[23] = Content.Load<Texture2D>("bR");

            for (int i = 24; i < 32; i++)
            {
                piece[i] = Content.Load<Texture2D>("bP");
            }

            for (int i = 0; i < 16; i++)
            {
                piece_position[i] = new Vector2(100 * (i % 8), (((int)i / 8) + 1) % 2 * 100 + 600);
            }

            for (int i = 16; i < 32; i++)
            {
                piece_position[i] = new Vector2(100 * (i % 8), ((int)(i / 8) - 2) * 100);
            }

            for(int i=0; i<32; i++)
            {
                elapsedTime[i] = 0;
                curent_size[i] = 1;
                animation_on[i] = false;
            }
        }

        protected void rysuj_czerowne_pola(Vector2[] pola)
        {
            for(int i=0; i< pola.Length; i++) 
            {
                Texture2D czerwony = Content.Load<Texture2D>("czerwone");

                _spriteBatch.Draw(czerwony, new Vector2(pola[i].X + board_offset_x, pola[i].Y+board_offset_y), new Color(255, 255, 255, 128)); // 50% przezroczystości

            }
        }
    }
    
}