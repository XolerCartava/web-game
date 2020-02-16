using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public interface IGameTurnRepository
    {
        IReadOnlyList<GameTurnEntity> GetLastTurns(Guid gameId, int maxCount);
        GameTurnEntity Insert(GameTurnEntity entity);
    }
}