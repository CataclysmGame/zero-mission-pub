using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KonamiCode : MonoBehaviour
{
    public UnityEvent performed;

    private enum Keys
    {
        None,
        Left,
        Right,
        Up,
        Down,
        A,
        B,
        Start,
    }

    private readonly Keys[] _konamiSequence =
    {
        Keys.Up, Keys.Up, Keys.Down, Keys.Down, Keys.Left, Keys.Right,
        Keys.Left, Keys.Right, Keys.B, Keys.A /*, Keys.Start, */
    };

    private GameControls _gameControls;
    private UIControls _uiControls;

    private int _sequenceIndex = 0;

    private void OnEnable()
    {
        _gameControls.Enable();
        _uiControls.Enable();
    }

    private void OnDisable()
    {
        _gameControls.Disable();
        _uiControls.Disable();
    }

    private void Awake()
    {
        _gameControls = new GameControls();
        _uiControls = new UIControls();

        MapInputAction(_gameControls.Gameplay.Fire, Keys.A);
        MapInputAction(_gameControls.Gameplay.Reload, Keys.B);
        MapInputAction(_uiControls.UI.Submit, Keys.Start);
        MapMoveAction(_gameControls.Gameplay.Move);
    }

    private void MapInputAction(InputAction inputAction, Keys key)
    {
        inputAction.performed += (_) => AddKey(key);
    }

    private void MapMoveAction(InputAction inputAction)
    {
        const float threshold = 0.8f;

        inputAction.performed += (_) =>
        {
            var movement = inputAction.ReadValue<Vector2>();
            if (movement.x < -threshold) AddKey(Keys.Left);
            else if (movement.x > threshold) AddKey(Keys.Right);
            else if (movement.y > threshold) AddKey(Keys.Up);
            else if (movement.y < -threshold) AddKey(Keys.Down);
        };
    }

    private void AddKey(Keys key)
    {
        if (key != _konamiSequence[_sequenceIndex])
        {
            _sequenceIndex = 0;
            return;
        }

        Logger.Log("Konami Code progress: " + key);

        _sequenceIndex++;

        if (_sequenceIndex == _konamiSequence.Length)
        {
            _sequenceIndex = 0;

            Logger.Log("Konami Code Performed");
            performed.Invoke();
        }
    }
}