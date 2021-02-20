using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Textures;
using Nez.UI;
using System;

namespace Game
{
    class InventoryMenu : Component, IUpdatable
    {
        VirtualButton _showMenu;

        UICanvas _canvas;
        Skin _skin;
        Table _table;
        Table _equipmentPane;
        Table _selectorPane;

        Inventory _playerInventory;

        public override void OnAddedToEntity()
        {
            _showMenu = new VirtualButton();
            _showMenu.AddKeyboardKey(Keys.Tab);
            _showMenu.AddGamePadButton(0, Buttons.Start);

            _skin = CreateSkin();
            _canvas = Entity.AddComponent<UICanvas>();
            _canvas.Enabled = false;
            _canvas.RenderLayer = RenderLayer.PlayerMenu;

            _table = _canvas.Stage.AddElement(new Table());
            _table.Top().Left().Pad(48);
            _table.Defaults().Top().SetPadRight(4);

            _playerInventory = Entity.Scene.GetPlayerInventory();

            BuildTable();
        }

        Button BuildTable()
        {
            _table.ClearChildren();

            _equipmentPane = new Table();
            _equipmentPane.Defaults().Width(96).Height(32);
            _table.Add(_equipmentPane);

            var weaponButton = BuildEquipmentMenu();

            // TODO: maybe a nested/separate table would be more appropriate
            _selectorPane = new Table();
            _selectorPane.Defaults().Width(96).Height(32);
            _table.Add(_selectorPane);

            return weaponButton;
        }

        Button BuildEquipmentMenu()
        {
            _equipmentPane.ClearChildren();

            var equippedWeapon = _playerInventory.EquippedWeapon;

            var weaponButton = CreateEquipmentButton(
                "MELEE", equippedWeapon?.Icon, equippedWeapon?.Name);
            _equipmentPane.Add(weaponButton);
            _equipmentPane.Row();

            var gunButton = CreateEquipmentButton("GUN", null, null);
            _equipmentPane.Add(gunButton);

            weaponButton.OnClicked += button =>
            {
                var elem = BuildWeaponMenu();
                _canvas.Stage.SetGamepadFocusElement(elem);
            };
            Action<ConfigurableButton> clearSelectorPane = button =>
            {
                _selectorPane.ClearChildren();
            };
            weaponButton.OnFocusedEvent += clearSelectorPane;
            gunButton.OnFocusedEvent += clearSelectorPane;

            return weaponButton;
        }

        Button BuildWeaponMenu()
        {
            _selectorPane.ClearChildren();

            Button first = null;
            for (var i = 0; i < _playerInventory.Weapons.Count; ++i)
            {
                var weapon = Weapon.Types[_playerInventory.Weapons[i]];
                var button = CreateEquipmentButton(weapon.Name, weapon.Icon, null);
                var index = i;
                button.OnClicked += btn =>
                {
                    _playerInventory.EquippedWeaponIndex = index;
                };
                _selectorPane.Add(button);
                _selectorPane.Row();
                if (first == null) first = button;
            }

            var unarmedButton = CreateEquipmentButton("Unarmed", null, null);
            unarmedButton.OnClicked += btn =>
            {
                _playerInventory.EquippedWeaponIndex = -1;
            };
            _selectorPane.Add(unarmedButton);
            if (first == null) first = unarmedButton;

            return first;
        }

        int _lastWeaponIndex = -1;
        public void Update()
        {
            if (_showMenu.IsPressed)
            {
                _canvas.Enabled = !_canvas.Enabled;

                if (_canvas.Enabled)
                {
                    Time.TimeScale = 0;
                    var firstButton = BuildTable();
                    _canvas.Stage.SetGamepadFocusElement(firstButton);
                }
                else
                {
                    Time.TimeScale = 1;
                }
            }

            if (_playerInventory.EquippedWeaponIndex != _lastWeaponIndex)
            {
                BuildEquipmentMenu();
            }

            _lastWeaponIndex = _playerInventory.EquippedWeaponIndex;
        }

        Skin CreateSkin()
        {
            var skin = new Skin();

            var buttonUp = new NinePatchDrawable(
                new NinePatchSprite(GameContent.LoadSprite("hud", "buttonUp", Core.Content), 1, 1, 1, 1));
            var buttonOver = new NinePatchDrawable(
                new NinePatchSprite(GameContent.LoadSprite("hud", "buttonOver", Core.Content), 1, 1, 1, 1));

            skin.Add(
                "default",
                new ButtonStyle(
                    buttonUp,
                    buttonUp,
                    buttonOver));

            return skin;
        }

        ConfigurableButton CreateEquipmentButton(string label, Sprite iconSprite, string name)
        {
            var button = new ConfigurableButton(_skin.Get<ButtonStyle>("default"));
            button.Defaults().Pad(2);

            var icon = new Table();
            icon.SetBackground(new NinePatchDrawable(
                new NinePatchSprite(GameContent.LoadSprite("hud", "buttonUp", Core.Content), 1, 1, 1, 1)));
            if (iconSprite != null)
                icon.Add(new Image(new SpriteDrawable(iconSprite)));
            button.Add(icon).Top().Left().Size(16);

            button.Add(new Label(label)).Expand().Top().Right();

            button.Row();
            button.Add();
            if (name != null)
                button.Add(name).Bottom().Right();

            return button;
        }
    }

    class ConfigurableButton : Button
    {
        public event Action<ConfigurableButton> OnFocusedEvent;

        public ConfigurableButton(ButtonStyle style)
            : base(style)
        {
        }

        protected override void OnFocused()
        {
            base.OnFocused();
            OnFocusedEvent?.Invoke(this);
        }
    }
}
