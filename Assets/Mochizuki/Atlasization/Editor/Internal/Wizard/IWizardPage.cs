/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using Mochizuki.Atlasization.Internal.Enum;
using Mochizuki.Atlasization.Internal.Models;

namespace Mochizuki.Atlasization.Internal.Wizard
{
    internal interface IWizardPage
    {
        WizardPage PageId { get; }

        void OnInitialize();

        bool OnGui(AtlasConfiguration configuration);

        void OnFinalize(AtlasConfiguration configuration);

        void OnDiscard();
    }
}