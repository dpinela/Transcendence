using HutongGames.PlayMaker;

namespace Transcendence
{
    internal static class PlayMakerExtensions
    {
        internal static FsmState AddState(this PlayMakerFSM fsm, string name)
        {
            var dest = new FsmState[fsm.FsmStates.Length + 1];
            Array.Copy(fsm.FsmStates, dest, fsm.FsmStates.Length);
            var newState = new FsmState(fsm.Fsm)
            {
                Name = name,
                Transitions = Array.Empty<FsmTransition>(),
                Actions = Array.Empty<FsmStateAction>()
            };
            dest[fsm.FsmStates.Length] = newState;
            fsm.Fsm.States = dest;
            return newState;
        }

        internal static FsmState GetState(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
        }

        internal static void RemoveAction(this FsmState s, int i)
        {
            var actions = new FsmStateAction[s.Actions.Length - 1];
            Array.Copy(s.Actions, actions, i);
            Array.Copy(s.Actions, i + 1, actions, i, s.Actions.Length - i - 1);
            s.Actions = actions;
        }

        internal static void FilterActions(this FsmState s, Func<FsmStateAction, bool> predicate)
        {
            s.Actions = s.Actions.Where(predicate).ToArray();
        }

        internal static void AddAction(this FsmState s, Action a)
        {
            SpliceAction(s, s.Actions.Length, a);
        }

        internal static void AppendAction(this FsmState s, Action a)
        {
            AddAction(s, a);
        }

        internal static void SpliceAction(this FsmState s, int pos, Action a)
        {
            var actions = new FsmStateAction[s.Actions.Length + 1];
            Array.Copy(s.Actions, actions, pos);
            actions[pos] = new FuncAction(a);
            Array.Copy(s.Actions, pos, actions, pos + 1, s.Actions.Length - pos);
            s.Actions = actions;
        }

        internal static void PrependAction(this FsmState s, Action a)
        {
            SpliceAction(s, 0, a);
        }

        internal static void ReplaceAction(this FsmState s, int i, Action a)
        {
            s.Actions[i] = new FuncAction(a);
        }

        internal static void AddTransition(this FsmState s, string eventName, string toState)
        {
            var transitions = new FsmTransition[s.Transitions.Length + 1];
            Array.Copy(s.Transitions, transitions, s.Transitions.Length);
            transitions[s.Transitions.Length] = new FsmTransition
            {
                FsmEvent = FsmEvent.GetFsmEvent(eventName),
                ToFsmState = s.Fsm.GetState(toState),
                ToState = toState,
            };
            s.Transitions = transitions;
        }

        internal static void RemoveAllTransitions(this FsmState s)
        {
            s.Transitions = new FsmTransition[0];
        }

        internal static FsmInt GetFsmInt(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmVariables.IntVariables.FirstOrDefault(v => v.Name == name);
        }

        internal static FsmBool GetFsmBool(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmVariables.BoolVariables.FirstOrDefault(v => v.Name == name);
        }
    }
}