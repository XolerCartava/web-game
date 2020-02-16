using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Game.Domain
{
    public class GameTurnEntity
    {
        public GameTurnEntity(Guid gameId, int turnIndex, Dictionary<string, PlayerDecision> playerDecisions, Guid winnerId)
            : this(Guid.Empty, gameId, turnIndex, playerDecisions, winnerId)
        {

        }

        [BsonConstructor]
        public GameTurnEntity(Guid id, Guid gameId, int turnIndex, Dictionary<string, PlayerDecision> playerDecisions, Guid winnerId)
        {
            Id = id;
            GameId = gameId;
            TurnIndex = turnIndex;
            this.playerDecisions = playerDecisions;
            WinnerId = winnerId;
        }

        [BsonElement]
        public Guid Id { get; private set; }

        [BsonElement]
        public Guid GameId { get; }

        [BsonElement]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        private readonly Dictionary<string, PlayerDecision> playerDecisions;

        [BsonElement]
        public Guid WinnerId { get; }

        [BsonElement]
        public int TurnIndex { get; }

        [BsonIgnore]
        public IReadOnlyDictionary<string, PlayerDecision> PlayerDecisions => playerDecisions;
    }
}