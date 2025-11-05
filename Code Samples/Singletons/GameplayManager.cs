using System.Collections;
using UnityEngine;

namespace Singletons
{
    public class GameplayManager : Singleton<GameplayManager>
    {
        public GameObject Player { get; private set; }
    }
}