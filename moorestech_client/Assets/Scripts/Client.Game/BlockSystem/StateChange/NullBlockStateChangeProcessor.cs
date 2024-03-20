﻿namespace Client.Game.BlockSystem.StateChange
{
    /// <summary>
    ///     無機能のブロックに使うステートプロセッサー
    /// </summary>
    public class NullBlockStateChangeProcessor : IBlockStateChangeProcessor
    {
        public void OnChangeState(string currentState, string previousState, string currentStateData)
        {
        }
    }
}