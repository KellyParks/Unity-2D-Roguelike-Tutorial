using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;
//need to do this because 'Random' exists in both UnityEngine and System namespaces
using Random = UnityEngine.Random;

/*
 This script is going to randomly generate levels each time the player starts a new level. 
*/
public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int minimum;
        public int maximum;

        public Count (int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    //These are for setting the size of the game board.
    public int columns = 8;
    public int rows = 8;

    /* 
     * Specify a random range for how many walls we want to spawn at each level.
     * In this case, we're saying spawn a minimum of 5 walls, to a maximum of 9 walls, per level.
     */
    public Count wallCount = new Count(5, 9);

    /*
     * Similarly to how we spawn the walls, we're going to set the minimum/maximum number of food
     * items for the board.
     */
    public Count foodCount = new Count(1, 5);

    //There's only one exit, so create a single GameObject for that prefab.
    public GameObject exit;
    
    /*
     * Create an array for the prefabs that have variations. This way, we can pass in multiple objects,
     * and choose one of them that we want to spawn. Each of these arrays will be filled with our different
     * prefabs to choose from in the inspector.
     */
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles;

    /*
     * We're going to be spawning a lot of game objects, and to keep the hierarchy clean, we're going to child all
     * those game objects to boardHolder.
     */
    private Transform boardHolder;

    /*
     * This tracks all the different possible positions on the game board. It keeps track of whether or not an object
     * has been spawned in that position.
     */
    private List<Vector3> gridPositions = new List<Vector3>();

    /*
     * Clears the gridPositions list to prepare it to generate a new board.
     */
    void InitialiseList()
    {
        gridPositions.Clear();

        /*
         * Fill the gridPositions list with each of the possible positions on our game board
         * to place walls, enemies or food. In each loop, we subtract one from both the row
         * and the column widths. This is to allow us to create a boarder of floor tiles on the
         * out-most side of the game board, so that the player doesn't become trapped.
         */
        for (int x = 1; x < columns-1; x++)
        {
            for(int y = 1; y < rows-1; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    /*
     * Sets up the outer wall and floor (background) of the game board.
     */
    void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;

        /*
         * The reason why each axes is going from -1 to the axes +1 is because we're building 
         * an edge around the active portion of the game board.
         */
        for(int x = -1; x < columns + 1; x++)
        {
            for(int y = -1; y < rows + 1; y++)
            {
                //Choose a floor tile at random from the floorTiles array, and prepare to instantiate it.
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                /*
                 * Next, check if we're in one of the outer wall positions. If so, randomly choose an outer 
                 * wall tile to instantiate. */
                if (x == -1 || x == columns || y == -1 || y == rows)
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                }

                /*
                 * toInstantiate is the prefab that we've chosen, and we're instantiating it at the x/y coordinates in the loop.
                 * Quaternion.identity means it's going to be instantiated with no rotation.
                 */
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                //set the parent of the newly instantiated game object to boardHolder.
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    /*
     * Returns a random position from the gridPositions list.
     */
    Vector3 RandomPosition()
    {
        //gridPositions.Count is the number of positions stored in our gridPositions list
        int randomIndex = Random.Range(0, gridPositions.Count);
        //declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
        Vector3 randomPosition = gridPositions[randomIndex];
        //to make sure we don't spawn two objects at the same location, we remove that grid position from our list.
        gridPositions.RemoveAt(randomIndex);
        //return the value of random position to that we can use it to spawn the object in a random location.
        return randomPosition;
    }

    /*
     * Accepts an array of game objects to choose from, along with a minimum and maximum range for the number of objects to create.
     */
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //this will controll how many of a given object we'll spawn (for example, the number of walls in a level).
        int objectCount = Random.Range(minimum, maximum + 1);

        //instantiate objects until the randomly chosen limit objectCount is reached
        for (int i = 0; i < objectCount; i++)
        {
            //choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
            Vector3 randomPosition = RandomPosition();

            //choose a random tile from tileArray and assign it to tileChoice
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

            //instantiate tileChoice at the position returned by RandomPosition with no change in rotation
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    /*
     * Initializes our level and calls the necessary functions to lay out the game board.
     */
    public void SetupScene(int level)
    {
        //creates the outer walls and floor
        BoardSetup();

        //reset gridpositions list
        InitialiseList();

        //instantiate a random number of wall tiles based on minimum and maximum, at randomized positions
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

        //instantiate a random number of food tiles based on minimum and maximum, at randomized positions
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

        //determine number of enemies based on current level, based on a logarithmic progression (ie: level number x 2)
        int enemyCount = (int)Mathf.Log(level, 2f);

        //instantiate number of enemies at randomized positions
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);

        //to make sure the exit always appears in the upper right corner, subtract 1 from both the columns and rows variables
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }
}
