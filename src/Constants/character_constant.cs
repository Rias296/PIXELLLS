public class Character_Constant{
    public static float MOVEMENT_SPEED = 250;
    public static float MAX_WALK_SPEED = 450;
    public static float MAX_RUN_SPEED = 700;
       public static float PLAYER_RUN_SOUND_DB = 60;
    public static float DIRECTIONAL_INTERVAL = 10f;
    public static float DIRECTION_CHANGE_TIMER = 0f;

    public enum CharacterStates{
        IDLE,
        MOVING,
        CHASING,
        SEARCHING,
        PATROLLING

    }

    
}