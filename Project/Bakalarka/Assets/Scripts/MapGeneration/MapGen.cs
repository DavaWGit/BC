/**
 * Autor: David Zahálka
 *
 * Generování mapy
 * 
 * Tento skript slouží k dynamickému generování mapy pomocí algoritmu Perlinova šumu.
 * Mapa se skládá z různých biomů, jako například oceán, poušť, kamenné vrcholy, sněžné pláně a les.
 * Dále skript umisťuje herní prvky jako vstup do dungeonu a objekty na mapě (stromy, kaktusy, keře, ...).
 * Mapa a objekty jsou generovány při prvním spuštění nebo načteny, pokud již existuje uložená mapa.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using UnityEngine.UI;

public class MapGen : MonoBehaviour
{
    public float scale = 10f;
    public float threshold = 0.5f;

    public Tilemap tilemap, waterTilemap;   //dvě tilemapy, jedna pro přístupnou mapu, druhá pro vodu
    public TileBase waterTile, sandTile, stoneTile, snowTile, forestTile, grassTile;    // políčka pro každý biom

    public GameObject dungeonEntrancePrefab, playerPrefab, bush1Prefab, bush2Prefab, 
    cactus1Prefab, cactus2Prefab, miniCactusPrefab, tree1Prefab, tree2Prefab, tree3Prefab, 
    boulder1Prefab, boulder2Prefab, boulder3Prefab, boulder4Prefab, smallRock1Prefab, smallRock2Prefab,
    snowRockPrefab, snowCapPrefab, signPrefab, logPrefab;   // prefaby pro všechny objekty na mapě

    const float ELEVATION_SCALE = 40.0f;
    const float MOISTURE_SCALE = 50.0f;
    const float HEAT_NOISE_SCALE = 50.0f;

    const int NOISE_OCTAVES = 2;

    public static bool mapGenerated = false;
    public bool firstStarted = true;
    public int levelIndex;
    public static MapGen instance;

    public RawImage heatMapDisplay;
    public RawImage moistMapDisplay;
    public RawImage heightMapDisplay;


    int moistureOctaves = 6;
    float moisturePersistence = 0.5f;
    float moistureLacunarity = 2.0f;

    void Awake()
    {

        if(instance == null)
        {
            instance = this;
        }
        tilemap.size = new Vector3Int(250, 250, 1);
        waterTilemap.size = new Vector3Int(250, 250, 1);
        // kontrola jestli můžu načíst mapu, jinak ji generuji
        string mapFilePath = Path.Combine(Application.persistentDataPath, $"Level {levelIndex}");
        if(File.Exists(mapFilePath))
        {
            TilemapManager.instance.LoadMap();
        }
        else
        {
            GenerateMap();
            PlaceSpawnPoint();
            PlaceDungeon();
            GenerateObjectsOnTilemap();
            UIController.instance.UpdateCoins();
            UIController.instance.ResetExpUI();
            TilemapManager.mapGenerated = true;
            firstStarted = false;
        }  
    }

    void GenerateMap()
    {
        // generování jednotlivých map (výšková, srážková, teplotní)
        float[,] heightMap = GenerateHeightmap();
        float[,] moistureMap = GenerateNoiseMap(MOISTURE_SCALE, moistureOctaves, moisturePersistence, moistureLacunarity);
        float[,] heatMap = GenerateHeatMap(tilemap.size.x, tilemap.size.y, 1.5f, HEAT_NOISE_SCALE);

        // vykreslovaní map do textu bakalarky, jeste to dam do prezentace (nemazat)
        /*Texture2D heatMapTexture = CreateHeatMapTexture(heatMap);
        Texture2D moistMapTexture = CreateMoistureMapTexture(moistureMap);
        Texture2D heightMapTexture = CreateMoistureMapTexture(heightMap);
        heatMapDisplay.texture = heatMapTexture;
        moistMapDisplay.texture = moistMapTexture;
        heightMapDisplay.texture = heightMapTexture;*/

        // průchod mapou po jednotlivých políčkách
        for (int x = 0; x < tilemap.size.x; x++)
        {
            for (int y = 0; y < tilemap.size.y; y++)
            {
                float xCoord = (float)x / tilemap.size.x;
                float yCoord = (float)y / tilemap.size.y;

                float heatValue = heatMap[x,y];
                float moistureValue = moistureMap[x, y];
                float heightValue = heightMap[x, y];
                
                // přiřazení biomu k políčku
                TileBase selectedTile = ChooseTexture(heightValue, moistureValue, heatValue, xCoord, yCoord);
          
                if(selectedTile == waterTile)
                {
                    waterTilemap.SetTile(new Vector3Int(x, y, 0), waterTile);
                }
                else
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), selectedTile);
                }
            }
        }
    }

    // generovani teplotní mapy
    float[,] GenerateHeatMap(int width, int height, float equatorBias, float noiseScale)
    {
        float[,] heatMap = new float[width, height];
        float halfHeight = height / 2f;
        System.Random prng = new System.Random();
        Vector2 noiseOffset = new Vector2(prng.Next(-10000, 10000), prng.Next(-10000, 10000));

        // krivka pro teplotu od rovniku k polům
        AnimationCurve temperatureCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        );
        temperatureCurve.preWrapMode = WrapMode.ClampForever;
        temperatureCurve.postWrapMode = WrapMode.ClampForever;

        float largeScale = 0.03f;
        float mediumScale = 0.07f;
        float smallScale = 0.05f;

        float largeInfluence = 0.5f;
        float smallInfluence = 0.1f;

        for (int y = 0; y < height; y++)
        {
            float latDist = Mathf.Abs(y - halfHeight) / halfHeight; // vzdálenost od rovníku
            latDist = Mathf.Pow(latDist, equatorBias);

            float noiseInfluence = Mathf.Lerp(largeInfluence, smallInfluence, latDist);

            for (int x = 0; x < width; x++)
            {
                float baseTemp = temperatureCurve.Evaluate(latDist);

                float largeNoise = Mathf.PerlinNoise((x + noiseOffset.x) * largeScale, (y + noiseOffset.y) * largeScale);
                float mediumNoise = Mathf.PerlinNoise((x + noiseOffset.x) * mediumScale, (y + noiseOffset.y) * mediumScale);
                float smallNoise = Mathf.PerlinNoise(x * smallScale, y * smallScale);

                largeNoise = (largeNoise - 0.5f) * 2 * noiseInfluence;
                mediumNoise = (mediumNoise - 0.5f) * 2 * noiseInfluence;
                smallNoise = (smallNoise - 0.5f) * 2 * noiseInfluence;
                
                float tempVariation = (largeNoise + mediumNoise + smallNoise) * noiseInfluence;
                float finalTemp = Mathf.Clamp01(baseTemp + tempVariation);

                heatMap[x, y] = finalTemp;
            }
        }

        return heatMap;
    }

    // generování mapy srážek
    float[,] GenerateNoiseMap(float noiseScale, int octaves, float persistence, float lacunarity)
    {
        float[,] noiseMap = new float[tilemap.size.x, tilemap.size.y];
        System.Random prng = new System.Random();

        // offsety v různých oktávách pro náhodnost
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + noiseScale;
            float offsetY = prng.Next(-100000, 100000) - noiseScale;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxPossibleHeight = 0;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }
        for (int y = 0; y < tilemap.size.y; y++)
        {
            for (int x = 0; x < tilemap.size.x; x++)
            {
                amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x + octaveOffsets[i].x) / noiseScale * frequency;
                    float sampleY = (y + octaveOffsets[i].y) / noiseScale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        // normalizace do rozsahu 0 až 1
        for (int y = 0; y < tilemap.size.y; y++)
        {
            for (int x = 0; x < tilemap.size.x; x++)
            {
                noiseMap[x, y] = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
            }
        }

        return noiseMap;
    }

    // generovani výškové mapy
    float[,] GenerateHeightmap()
    {
        float[,] heightmap = new float[tilemap.size.x, tilemap.size.y];
        float edgeFalloffStart = 0.01f; // falloff na okrajích mapy
        float edgeFalloffEnd = 0.2f;    // konec falloffu
        float halfWidth = tilemap.size.x * 0.5f;
        float halfHeight = tilemap.size.y * 0.5f;
        float squareness = 0.7f;
        float maxDistance = Mathf.Min(halfWidth, halfHeight);

        float minHeightValue = float.PositiveInfinity;
        float maxHeightValue = float.NegativeInfinity;

        for (int y = 0; y < tilemap.size.y; y++)
        {
            for (int x = 0; x < tilemap.size.x; x++)
            {
                float noiseValue = GetNoise(x, y, 0.01f, 3);

                float xDistance = Mathf.Abs(x - halfWidth) / halfWidth;
                float yDistance = Mathf.Abs(y - halfHeight) / halfHeight;
                float squareDistance = Mathf.Max(xDistance, yDistance);
                float distance = Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance) / Mathf.Sqrt(2); // Diagonal distance
                float falloff = Mathf.SmoothStep(1.0f, 0.0f, Mathf.InverseLerp(edgeFalloffStart, edgeFalloffEnd, distance));

                float circularDistance = Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance);
                float combinedDistance = Mathf.Lerp(circularDistance, squareDistance, squareness);

                float heightValue = noiseValue * (1.0f - Mathf.Clamp01(combinedDistance));

                heightValue = Mathf.Clamp(heightValue, 0.0f, 1.0f);

                if (heightValue < minHeightValue) minHeightValue = heightValue;
                if (heightValue > maxHeightValue) maxHeightValue = heightValue;

                heightmap[x, y] = heightValue;
            }
        }
        for (int x = 0; x < tilemap.size.x; x++)
        {
            for (int y = 0; y < tilemap.size.y; y++)
            {
                heightmap[x, y] = Mathf.InverseLerp(minHeightValue, maxHeightValue, heightmap[x, y]);
            }
        }

        return heightmap;
    }

    // generuje šum z více vrstev perlinova šumu
    float GetNoise(float x, float y, float scale, int layers)
    {
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float noise = 0.0f;
        float maxAmplitude = 0.0f;

        for (int i = 0; i < layers; i++)
        {
            noise += Mathf.PerlinNoise(x * scale * frequency, y * scale * frequency) * amplitude;
            maxAmplitude += amplitude;
            amplitude *= 0.5f;
            frequency *= 2.0f;
        }

        return noise / maxAmplitude;
    }

    Texture2D CreateHeatMapTexture(float[,] heatMap)
    {
        int width = heatMap.GetLength(0);
        int height = heatMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = heatMap[x, y];
                Color color = new Color(value, 0f, 1f - value);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    Texture2D CreateMoistureMapTexture(float[,] moistureMap)
    {
        int width = moistureMap.GetLength(0);
        int height = moistureMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = moistureMap[x, y];
                Color color = new Color(value, value, value);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    // funkce na výběr biomu
    TileBase ChooseTexture(double height, double moisture, double heat, double x, double y)
    {
        // inicializace hodnot pro jednotlivé biomy
        const double WATER_LEVEL = 0.025;
        const double BEACH_LEVEL = 0.045;
        const double GRASSLAND_LEVEL = 0.6;
        const double MOUNTAIN_LEVEL = 0.8;
        const double POLE_HEAT_LEVEL = 0.25;
        const double SAND_MOISTURE_LEVEL = 0.35;
        const double SAND_HEAT_LEVEL = 0.8;

        if (height < WATER_LEVEL) return waterTile;

        if (heat > SAND_HEAT_LEVEL && moisture < SAND_MOISTURE_LEVEL && !(y > 0.75 || y < 0.25)) return sandTile;
        
        if (heat > SAND_HEAT_LEVEL && moisture > 0.95 && !(y > 0.75 || y < 0.25)) return waterTile;

        if (heat < POLE_HEAT_LEVEL && height > BEACH_LEVEL && !(y > 0.35 && y < 0.65)) 
        {
            if(moisture > 0.35)
            {
                return snowTile;
            }
            else
            {
                return stoneTile;
            }
        }
        if(heat > POLE_HEAT_LEVEL && heat < 0.35 && (y > 0.8 || y < 0.2))
        {
            float isSnow = Random.Range(0.0f, 1.0f);
            if(isSnow - heat > 0.6f && height > BEACH_LEVEL)
            {
                return snowTile;
            }
        }
        if (height < BEACH_LEVEL) return sandTile;

        if (height > BEACH_LEVEL && height < MOUNTAIN_LEVEL && heat < SAND_HEAT_LEVEL)
        {
            if (moisture > GRASSLAND_LEVEL && moisture < 0.9) return grassTile;
            else if(moisture > 0.9) return waterTile;
            else return forestTile;
        }
        if(heat < 0.3)
        {
            return snowTile;
        }
        return forestTile;

    }

    // generování objektů na mapě (stromy, keře, ...)
    void GenerateObjectsOnTilemap()
    {
        float[,] objectMap = GenerateObjectMap(50, 150);
        float objectToGenerate;
        for (int x = 0; x < tilemap.size.x; x++)
        {
            for (int y = 0; y < tilemap.size.y; y++)
            {
                Vector3Int tilemapPosition = new Vector3Int(x, y, 0);
                Vector3 worldPosition = tilemap.GetCellCenterWorld(tilemapPosition);
                TileBase tile = tilemap.GetTile(tilemapPosition);

                if (tile != null)
                {
                    float max = 0;

                    for (int dy = -3; dy <= 3; dy++)
                    {
                        for (int dx = -3; dx <= 3; dx++)
                        {
                            int xn = dx + x, yn = dy + y;
                            if (0 <= yn && yn < tilemap.size.y && 0 <= xn && xn < tilemap.size.x)
                            {
                                float e = objectMap[xn, yn];
                                if (e > max)
                                {
                                    max = e;
                                }
                            }
                        }
                    }
                    if(CanGenerateObject(tilemapPosition))
                    {
                        if(objectMap[x,y] == max)
                        {
                            if(!IsCollidingWithOtherProps(tilemapPosition, 10))
                            {
                                switch(tile.name)
                                {
                                    case "sand":
                                        objectToGenerate = Random.Range(0.0f, 1.0f);
                                        if (objectToGenerate <= 0.2f)
                                        {
                                            Instantiate(bush1Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if (objectToGenerate <= 0.4f)
                                        {
                                            Instantiate(bush2Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if (objectToGenerate <= 0.6f)
                                        {
                                            Instantiate(cactus1Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if (objectToGenerate <= 0.8f)
                                        {
                                            Instantiate(cactus2Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else
                                        {
                                            Instantiate(miniCactusPrefab, worldPosition, Quaternion.identity);
                                        }
                                        break;
                                    case "grass":
                                    case "forest":
                                        objectToGenerate = Random.Range(0.0f, 1.0f);
                                        if(objectToGenerate <= 0.25)
                                        {
                                            GameObject randomObject = Instantiate(tree1Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.5)
                                        {
                                            GameObject randomObject = Instantiate(tree2Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.75)
                                        {
                                            GameObject randomObject = Instantiate(tree3Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.79)
                                        {
                                            GameObject randomObject = Instantiate(boulder1Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.83)
                                        {
                                            GameObject randomObject = Instantiate(boulder2Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.87)
                                        {
                                            GameObject randomObject = Instantiate(boulder3Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.91)
                                        {
                                            GameObject randomObject = Instantiate(boulder4Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.95)
                                        {
                                            GameObject randomObject = Instantiate(smallRock1Prefab, worldPosition, Quaternion.identity);
                                        }
                                        else
                                        {
                                            GameObject randomObject = Instantiate(smallRock2Prefab, worldPosition, Quaternion.identity);
                                        }
                                        break;
                                    case "snow":
                                        objectToGenerate = Random.Range(0.0f, 1.0f);
                                        if(objectToGenerate <= 0.5)
                                        {
                                            GameObject randomObject = Instantiate(snowCapPrefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.7)
                                        {
                                            GameObject randomObject = Instantiate(logPrefab, worldPosition, Quaternion.identity);
                                        }
                                        else if(objectToGenerate <= 0.95)
                                        {
                                            GameObject randomObject = Instantiate(snowRockPrefab, worldPosition, Quaternion.identity);
                                        }
                                        else
                                        {
                                            GameObject randomObject = Instantiate(signPrefab, worldPosition, Quaternion.identity);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // mapa pomocí perlinova šumu pro objekty
    float[,] GenerateObjectMap(float scale, int seed)
    {
        float[,] noiseMap = new float[tilemap.size.x, tilemap.size.y];

        for (int x = 0; x < tilemap.size.x; x++)
        {
            for (int y = 0; y < tilemap.size.y; y++)
            {
                float xCoord = (float)x / tilemap.size.x * scale + seed;
                float yCoord = (float)y / tilemap.size.y * scale + seed;

                float value = Mathf.PerlinNoise(xCoord, yCoord);

                noiseMap[x, y] = value;
            }
        }

        return noiseMap;
    }
    // kontrola zda může být generován objekt
    bool CanGenerateObject(Vector3Int tilemapPosition)
    {
        TileBase tile = waterTilemap.GetTile(tilemapPosition);

        if (tile == waterTile || IsNextToWater(tilemapPosition))
        {
            return false;
        }
        return true;
    }
    // kontrola přítomnosti vody
    bool IsNextToWater(Vector3Int tilemapPosition)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Vector3Int neighborTilemapPosition = tilemapPosition + new Vector3Int(i, j, 0);

                TileBase neighborTile = waterTilemap.GetTile(neighborTilemapPosition);

                if (neighborTile == waterTile)
                {
                    return true;
                }
            }
        }
        return false;
    }
    // kontrola kolize s jinými objekty
    bool IsCollidingWithOtherProps(Vector3Int position, float minDistance)
    {
        Vector2 position2D = new Vector2(position.x, position.y);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position2D, new Vector2(minDistance, minDistance), 0);
        return colliders.Length > 0;
    }
    // položení spawn pointu hráče
    void PlaceSpawnPoint()
    {
        List<Vector3Int> spawnLocations = new List<Vector3Int>();

        for (int x = 0; x < tilemap.size.x; x++)
        {
            for (int y = 0; y < tilemap.size.y; y++)
            {
                Vector3Int tilemapPosition = new Vector3Int(x, y, 0);
                if (CanGenerateObject(tilemapPosition))
                {
                    spawnLocations.Add(tilemapPosition);
                }
            }
        }

        if (spawnLocations.Count > 0)
        {
            Vector3Int chosenLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];
            Vector3 worldPosition = tilemap.GetCellCenterWorld(chosenLocation);
            GameObject playerInstance = Instantiate(playerPrefab, worldPosition, Quaternion.identity);
        
        }
    }
    // položení vchodu do podzemí
    void PlaceDungeon()
    {
        List<Vector3Int> dungeonLocations = new List<Vector3Int>();

        for (int x = 0; x < tilemap.size.x; x++)
        {
            for (int y = 0; y < tilemap.size.y; y++)
            {
                Vector3Int tilemapPosition = new Vector3Int(x, y, 0);
                if (CanGenerateObject(tilemapPosition))
                {
                    dungeonLocations.Add(tilemapPosition);
                }
            }
        }

        if (dungeonLocations.Count > 0)
        {
            Vector3Int chosenLocation = dungeonLocations[Random.Range(0, dungeonLocations.Count)];
            GameObject dungeonEntrance = Instantiate(dungeonEntrancePrefab, tilemap.GetCellCenterWorld(chosenLocation), Quaternion.identity);
        }
    }
}
