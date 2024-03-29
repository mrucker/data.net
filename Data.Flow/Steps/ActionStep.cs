﻿using System;

namespace Data.Flow
{
    public class ActionStep: Step
    {
        #region Properties
        public Action Action { get; }
        #endregion

        #region Constructor
        public ActionStep(Action action)
        {
            Action = action;
        }
        #endregion

        #region Overrides
        protected override void Initializing()
        {

        }

        protected override void Processing()
        {
            using (Tracker.Whole(1))
            using (Tracker.Piece())
            {
                Action();
            }
        }
        #endregion
    }
}