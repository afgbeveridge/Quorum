﻿#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
namespace Quorum {

    public static class EventNames {

        public const string Discovery = "Discovery";
        public const string Query = "Query";
        public const string Active = "Active";
        public const string Die = "Die";
        public const string NeighbourDying = "NeighbourDying";
        public const string Quiescent = "Quiescent";
        public const string RequestElection = "RequestElection";
        public const string ExternalRequestElection = "ExternalRequestElection";
        public const string ElectionResult = "ElectionResult";
        public const string Elected = "Elected";
        public const string NotElected = "NotElected";
        public const string VoteCast = "VoteCast";
        public const string Abdication = "Abdication";
        public const string MakePretender = "MakePretender";
        public const string ConfigurationOffered = "ConfigurationOffered";

    }

}
