// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;

namespace osu.Framework.Graphics.UserInterface
{
    public class BasicRearrangeableItem : RearrangeableListItem
    {
        public string Text;

        public BasicRearrangeableItem(string text)
        {
            Text = text;
        }
    }
}