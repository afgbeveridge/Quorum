#region License
//
// Copyright Tony Beveridge 2015. All rights reserved. 
// MIT license applies.
//
#endregion
using FSM;

namespace ControlCentre {

    public static class Builder {

        public static void ConfigureInjections() {
            Container = new Quorum.Builder();
            Container.CreateBaseRegistrations();
        }

        public static TType Resolve<TType>() {
            return Container.Resolve<TType>();
        }

        internal static Quorum.Builder Container { get; set; }

        internal static IContainer AsContainer { get { return Container.AsContainer(); } }

    }

}
