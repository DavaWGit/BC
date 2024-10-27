/**
 * Autor: David Zahálka
 *
 * Generování mapy dungeonu (podzemí)
 * 
 * Tento skript používá algoritmus celulárních automatů pro generování dungeonů. Inicializuje
 * mapu dungeonu s náhodnými zdmi a podlahami, poté iterativně aplikuje limity narození a smrti
 * pro simulaci místností a chodeb dungeonu. Východ z dungeonu je dynamicky umístěn na otevřeném místě.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class DungeonGen : MonoBehaviour
{
    [Range(0,100)]
    public int inChance;    // pravděpodobnost, že bude políčko začínat jako zeď
    [Range(1,8)]
    public int birthLim;    // limit pro zarození zdi
    [Range(1,8)]
    public int deathLim;    // limit pro smrt zdi

    [Range(1,10)]
    public int numR;
    //private int count = 0;

    private int[,] tileMap;
    public Vector3Int mapSize;

    public Tilemap wallMap;
    public Tilemap floorMap;
    public Tile[] wallTiles;
    public Tile floorTile;

    public GameObject dungeonExit;
    private GameObject exitInstance;

    int width;
    int height;

    public int levelIndex;

    // Start is called before the first frame update
    void Start()
    {
        // načteni existující mapy, jinak generování nové
        string mapFilePath = Path.Combine(Application.persistentDataPath, $"Level {levelIndex}");
        if(File.Exists(mapFilePath))
        {
            TilemapManager.instance.LoadMap();
        }
        else
        {
            StartGen(numR);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGen(int numR)
    {
        clearGenMap(false);
        width = mapSize.x;
        height = mapSize.y;

        if(tileMap == null)
        {
            tileMap = new int[width, height];
            createMap();
        }

        for(int i = 0; i < numR; i++)
        {
            tileMap = createTiles(tileMap); // aplikace pravidel numR cyklů
        }


        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                floorMap.SetTile(new Vector3Int(-x + width/2, -y + height/2, 0), floorTile);
                if(tileMap[x,y] == 1)
                {
                    int wallType = SetWall(tileMap, x, y);
                    wallMap.SetTile(new Vector3Int(-x + width/2, -y + height/2, 0), wallTiles[wallType]);
                }
            }
        }

        // umístění východu z podzemí
        int exitX;
        int exitY;
        do
        {
            exitX = Random.Range(0, width);
            exitY = Random.Range(0, height);
        } while (tileMap[exitX, exitY] != 0);
        if (tileMap[exitX, exitY] == 0)
        {
            Vector3 exitPosition = floorMap.CellToWorld(new Vector3Int(-exitX + width / 2, -exitY + height / 2, 0));
            exitInstance = Instantiate(dungeonExit, exitPosition, Quaternion.identity);
            exitInstance.GetComponent<DungeonEntrance>().dungeonToLoad = "Main";
            exitInstance.GetComponent<DungeonEntrance>().areaTransitionName = "Dungeon";
        }
    }

    public int[,] createTiles(int[,] map)
    {
        int[,] newMap = new int[width, height];
        int neighbor;
        BoundsInt bounds = new BoundsInt(-1, -1, 0, 3, 3, 1);

        // aplikace pravidel pro každou buňku
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                neighbor = 0;
                foreach(var b in bounds.allPositionsWithin)
                {
                    if(b.x == 0 && b.y == 0)
                    {
                        continue;
                    }
                    if(x+b.x >= 0 && x+b.x < width && y+b.y >= 0 && y+b.y < height)
                    {
                        neighbor += map[x+b.x, y+b.y];
                    }
                    else
                    {
                        neighbor++;
                    }
                }
                if(map[x,y] == 1)
                {
                   if(neighbor < deathLim)
                   {
                        newMap[x,y] = 0;
                   } 
                   else
                   {
                        newMap[x,y] = 1;
                   }
                }
                if(map[x,y] == 0)
                {
                    if(neighbor > birthLim)
                    {
                        newMap[x,y] = 1;
                    }
                    else
                    {
                        newMap[x,y] = 0;
                    }
                }
            }
        }

        return newMap;
    }

    private int SetWall(int[,] map, int x, int y)
    {
        bool northWall = y + 1 < height && map[x, y + 1] == 1;
        bool southWall = y - 1 >= 0 && map[x, y - 1] == 1;
        bool eastWall = x + 1 < width && map[x + 1, y] == 1;
        bool westWall = x - 1 >= 0 && map[x - 1, y] == 1;

        if (northWall && southWall && eastWall && westWall)
        {
            return 0; // střed
        }
        else if (northWall && southWall && eastWall && !westWall)
        {
            return 1; // západ
        }
        else if (northWall && southWall && !eastWall && westWall)
        {
            return 2; // východ
        }
        else if (northWall && !southWall && eastWall && westWall)
        {
            return 3; // jih
        }
        else if (!northWall && southWall && eastWall && westWall)
        {
            return 4; // sever
        }
        else if (northWall && southWall && !eastWall && !westWall)
        {
            return 5; // západ-východ
        }
        else if (!northWall && !southWall && eastWall && westWall)
        {
            return 6; // sever-jih
        }
        else if (northWall && !southWall && !eastWall && westWall)
        {
            return 7; // jiho-východ
        }
        else if (northWall && !southWall && eastWall && !westWall)
        {
            return 8; // jiho-západ
        }
        else if (!northWall && southWall && eastWall && !westWall)
        {
            return 9; // severo-západ
        }
        else if (!northWall && southWall && !eastWall && westWall)
        {
            return 10; // severo-východ
        }
        else if (!northWall && !southWall && eastWall && !westWall)
        {
            return 11; // východ (bez sousedů)
        }
        else if (!northWall && !southWall && !eastWall && westWall)
        {
            return 12; // západ (bez sousedů)
        }
        else if (northWall && !southWall && !eastWall && !westWall)
        {
            return 13; // jih (bez sousedů)
        }
        else if (!northWall && southWall && !eastWall && !westWall)
        {
            return 14; // sever (bez sousedů)
        }
        else if (!northWall && !southWall && !eastWall && !westWall)
        {
            return 15; // izolovaná stěna
        }

        return 0;
    }

    public void createMap()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                tileMap[x, y] = Random.Range(1,101) < inChance ? 1 : 0;
            }
        }
    }

    public void clearGenMap(bool isDone)
    {
        wallMap.ClearAllTiles();
        floorMap.ClearAllTiles();

        if(isDone)
        {
            tileMap = null;
        }
    }
}
