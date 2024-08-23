using NetworkCode;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utilities;
using UnityEngine.Tilemaps;


/* ------------------------------------------- */
public struct InputPayload : INetworkSerializable
{
    public int tick;
    public Vector3 inputVector;
    public DateTime timestamp;
    public ulong networkObjectId;
    public Vector3 position;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref inputVector);
        serializer.SerializeValue(ref timestamp);
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref position);
    }
}


/* ------------------------------------------- */
public struct StatePayload : INetworkSerializable
{
    public int tick;
    public Vector3 position;
    public ulong networkObjectId;
    public Quaternion rotation;
    public Vector3 velocity;
    public float angularVelocity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref tick);
        serializer.SerializeValue(ref position);
        serializer.SerializeValue(ref networkObjectId);
        serializer.SerializeValue(ref rotation);
        serializer.SerializeValue(ref velocity);
        serializer.SerializeValue(ref angularVelocity);
    }
}


/* ------------------------------------------- */
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private PlayerInputSystem playerInput;
    [SerializeField] private Vector2 direction;
    private Vector3 originalPosition, targetPosition;
    private bool isMoving;
    [SerializeField] TileMapRef timeMapSO;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap collisionTilemap;

    [Header("Network")]
    [SerializeField] private AuthorityMode authorityMode;

    private NetworkTimer networkTimer;
    private const float SERVERTICKRATE = 60f; //60 fps
    private const int BUFFERSIZE = 1024;

    [Header("Netcode Client Specific")]
    private CircularBuffer<StatePayload> clientStateBuffer;
    private CircularBuffer<InputPayload> clientInputBuffer;
    private StatePayload lastServerState;
    private StatePayload lastProcessedState;

    [Header("Netcode Server Specific")]
    private CircularBuffer<StatePayload> serverStateBuffer;
    private Queue<InputPayload> serverInputQueue;

    [Header("Reconciliation")]
    [SerializeField] private double reconciliationThreshold = 10f;
    [SerializeField] private float reconciliationCooldownTime = 1f;

    private CountdownTimer reconciliationCooldown;


    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        networkTimer = new NetworkTimer(SERVERTICKRATE);

        clientStateBuffer = new CircularBuffer<StatePayload>(BUFFERSIZE);
        clientInputBuffer = new CircularBuffer<InputPayload>(BUFFERSIZE);

        serverStateBuffer = new CircularBuffer<StatePayload>(BUFFERSIZE);
        serverInputQueue = new Queue<InputPayload>();

        reconciliationCooldown = new CountdownTimer(reconciliationCooldownTime);
        isMoving = false;
    }

    private void Start()
    {
        groundTilemap = timeMapSO.ground;
        collisionTilemap = timeMapSO.collision;
    }
    void Update()
    {
        direction = playerInput.direction;

        networkTimer.Update(Time.deltaTime); //Timer is a class that have the update method to update the timer
        reconciliationCooldown.Tick(Time.deltaTime);

    }

    void FixedUpdate()
    {
        while (networkTimer.ShouldTick()) //ShouldTick() returns true everytime the timer is bigger than the minimun tick timer
        {
            HandleClientTick();
            HandleServerTick();
        }
    }
    #region Basic Movement
    //Process the movement the player wants to do and return an state to save for later
    StatePayload ProcessMovement(InputPayload input)
    {
        MoveServerRpc(input.inputVector);

        return new StatePayload()
        {
            tick = input.tick,
            networkObjectId = NetworkObjectId,
            position = transform.position,
            rotation = transform.rotation,
            velocity = rigidBody.velocity,
            angularVelocity = rigidBody.angularVelocity
        };
    }

    [Rpc(SendTo.Server)]
    public void MoveServerRpc(Vector3 input)
    {
        if (!CanMove(input))
        {
            rigidBody.velocity = Vector3.zero;
            return;
        }
            

        Move(input);

    }
    private void Move(Vector3 input)
    {
        //Grid Movement
        //float elapsedTime = 0;

        //isMoving = true;

        //originalPosition = transform.position;
        //targetPosition = originalPosition + input;

        //while (elapsedTime < timeToMove)
        //{
        //    transform.position = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / timeToMove));
        //    elapsedTime += Time.deltaTime;
        //    yield return null;
        //}

        //transform.position = targetPosition;
        //isMoving = false;
        rigidBody.velocity = input * moveSpeed;
    }
    private bool CanMove(Vector3 input)
    {
        //Maybe add something in the future 
        return true;
    }
    #endregion

    #region Reconciliation
    private void HandleServerTick() //while there's input to read add to the buffer and process this movement
    {
        if (!IsServer)
            return;

        InputPayload inputPayload = default;
        var bufferIndex = -1;
        while (serverInputQueue.Count > 0) 
        {
            inputPayload = serverInputQueue.Dequeue();

            bufferIndex = inputPayload.tick % BUFFERSIZE;

            StatePayload statePayload = ProcessMovement(inputPayload);

            serverStateBuffer.Add(statePayload, bufferIndex); //Adding the state we are modifying to the buffer of the specified index

        }

        if (bufferIndex == -1)
            return;

        SendToClientRpc(serverStateBuffer.Get(bufferIndex));
    }

    [Rpc(SendTo.NotServer)]
    void SendToClientRpc(StatePayload statePayload)
    {
        if (!IsOwner)
            return;
        lastServerState = statePayload;
    }

    //Take the input and process a movement with it, in the end put the input to a buffer for the server to look at it after, and check it with the state
    private void HandleClientTick()
    {
        if (!IsClient || !IsOwner)
            return;

        var currentTick = networkTimer.CurrentTick; //take the tick we are in
        var bufferIndex = currentTick % BUFFERSIZE; //Find the index of the buffer

        InputPayload inputPayload = new InputPayload()
        {
            tick = currentTick,
            timestamp = DateTime.Now,
            networkObjectId = NetworkObjectId,
            inputVector = direction,
            position = transform.position
        };

        clientInputBuffer.Add(inputPayload, bufferIndex);
        SendToServerRpc(inputPayload);

        StatePayload statePayload = ProcessMovement(inputPayload);
        clientStateBuffer.Add(statePayload, bufferIndex);

        HandleServerReconciliation();
    }

    [Rpc(SendTo.Server)]
    private void SendToServerRpc(InputPayload input) 
    {
        serverInputQueue.Enqueue(input);
    }
    private void HandleServerReconciliation() //Check if the position the player are in should be reconciliate by the server
    {
        if (!ShouldReconcile())
            return;

        float positionError;
        int bufferIndex;
        StatePayload rewindState = default;

        bufferIndex = lastServerState.tick % BUFFERSIZE;

        if (bufferIndex - 1 < 0) //Not enough information
            return;

        rewindState = IsHost ? serverStateBuffer.Get(bufferIndex - 1) : lastServerState; //Host RPCs execute immediately, so we can use the last server state

        StatePayload clientState = IsHost ? clientStateBuffer.Get(bufferIndex - 1) : clientStateBuffer.Get(bufferIndex);
        positionError = Vector3.Distance(rewindState.position, clientState.position);

        if (positionError > reconciliationThreshold)
        {
            ReconcileState(rewindState);
            reconciliationCooldown.Start();
        }
        lastProcessedState = rewindState;
    }

    private bool ShouldReconcile() //Check if we should reconcile and return the bool value for that
    {
        bool isNewServerState = !lastServerState.Equals(default);

        bool isLastStateUnderfinedOrDifferent = lastProcessedState.Equals(default)
                                                || !lastProcessedState.Equals(lastServerState);

        return isNewServerState && isLastStateUnderfinedOrDifferent && !reconciliationCooldown.IsRunning;
    }

    void ReconcileState(StatePayload rewindState)
    {
        transform.position = rewindState.position;
        transform.rotation = rewindState.rotation;
        rigidBody.velocity = rewindState.velocity;
        rigidBody.angularVelocity = rewindState.angularVelocity;

        if (!rewindState.Equals(lastServerState))
            return;

        clientStateBuffer.Add(rewindState, rewindState.tick % BUFFERSIZE);

        //replay all inputs front the rewind state to the current state
        int tickToReplay = lastServerState.tick;

        while (tickToReplay < networkTimer.CurrentTick)
        {
            int bufferIndex = tickToReplay % BUFFERSIZE;
            StatePayload statePayload = ProcessMovement(clientInputBuffer.Get(bufferIndex));
            clientStateBuffer.Add(statePayload, bufferIndex);
            tickToReplay++;
        }
    }
    #endregion



}
