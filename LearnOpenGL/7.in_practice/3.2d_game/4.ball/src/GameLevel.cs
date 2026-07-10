using System.Numerics;

namespace Breakout;

public class GameLevel
{
    // level state
    List<GameObject> Bricks = new List<GameObject>();

    // constructor
    public GameLevel()
    {
        
    }

    // carrega a fase a partir de um arquivo
    public void Load(string file, uint levelWidth, uint levelHeight)
    {
        // clear old data
        Bricks.Clear();
        
        // load from file
        List<List<uint>> tileData = new List<List<uint>>();

        if (File.Exists(file))
        {
            using (StreamReader fstream = new StreamReader(file)) // MOVER PARA DENTRO DO IF
            {
                string line;
                while ((line = fstream.ReadLine()!) != null)
                {
                    string[] numbers = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    List<uint> row = new List<uint>();

                    foreach (string number in numbers)
                    {
                        if (uint.TryParse(number, out uint tileCode))
                        {
                            row.Add(tileCode);
                        }
                    }

                    if (row.Count > 0)
                    {
                        tileData.Add(row); // ADICIONAR A LINHA A tileData
                    }
                }
            }
            
            // CHAMAR Init DEPOIS de ler tudo
            if (tileData.Count > 0)
            {
                Init(tileData, levelWidth, levelHeight);
            }
        }
    }

    // render level
    public void Draw(SpriteRenderer renderer)
    {
        foreach (GameObject tile in Bricks)
        {
            if (!tile.Destroyed)
            {
                tile.Draw(renderer);
            }
        }
    }

    // verifica se a fase foi concluída (todos os blocos não sólidos foram destruídos)
    public bool IsCompleted()
    {
        foreach (GameObject tile in Bricks)
        {
            if (!tile.IsSolid && !tile.Destroyed)
            {
                return false;
            }
        }

        return true;
    }

    // inicializa o nível a partir dos dados de tiles
    private void Init(List<List<uint>> tileData, uint levelWidth, uint levelHeight)
    {
        // calcular dimensões
        uint height = (uint)tileData.Count();
        uint width = (uint)tileData[0].Count();

        float uint_widht = (float)levelWidth / (float)width;
        float uint_height = (float)levelHeight / (float)height;

        // inicializa os blocos do nível com base em tileData
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // verifica o tipo de bloco a partir dos dados do nível (matriz 2D do nível)
                if (tileData[y][x] == 1) // solid
                {
                    Vector2 pos = new Vector2(uint_widht * x, uint_height * y);
                    Vector2 size = new Vector2(uint_widht, uint_height);

                    GameObject obj = new GameObject(
                        pos, 
                        size,
                        ResourceManager.GetTexture("block_solid"),
                        new Vector3(0.8f, 0.8f, 0.7f)
                    );
                    obj.IsSolid = true;
                    Bricks.Add(obj);
                }
                else if (tileData[y][x] > 1)
                {
                    Vector3 color = new Vector3(1.0f); // original: branco

                    if (tileData[y][x] == 2)
                    {
                        color = new Vector3(0.2f, 0.6f, 1.0f);
                    }
                    else if (tileData[y][x] == 3)
                    {
                        color = new Vector3(0.0f, 0.7f, 0.0f);
                    }
                    else if (tileData[y][x] == 4)
                    {
                        color = new Vector3(0.8f, 0.8f, 0.4f);
                    }
                    else if (tileData[y][x] == 5)
                    {
                        color = new Vector3(1.0f, 0.5f, 0.0f);
                    }

                    Vector2 pos = new Vector2(uint_widht * x, uint_height * y);
                    Vector2 size = new Vector2(uint_widht, uint_height);

                    Bricks.Add(
                        new GameObject(
                            pos, 
                            size, 
                            ResourceManager.GetTexture("block"),
                            color
                        )
                    );
                }
            }
        }
    }
}
