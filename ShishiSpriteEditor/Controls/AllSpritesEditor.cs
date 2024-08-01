using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;

namespace FFTPatcher.SpriteEditor
{
    public partial class AllSpritesEditor : UserControl
    {
        public AllSprites Sprites { get; private set; }
        private Stream iso;

        public Sprite CurrentSprite
        {
            get { return spriteEditor1.Sprite; }
        }

        public int PaletteIndex
        {
            get { return spriteEditor1.PaletteIndex; }
        }

        public bool ImportExport8Bpp
        {
            get { return spriteEditor1.ImportExport8Bpp; }
        }

        public void BindTo(AllSprites allSprites, Stream iso)
        {
            if (allSprites == null)
                throw new ArgumentNullException("allSprites");
            if (iso == null)
                throw new ArgumentNullException("iso");
            if (!iso.CanRead || !iso.CanWrite || !iso.CanSeek)
                throw new InvalidOperationException("iso doesn't support reading, writing, and seeking");

            this.Sprites = allSprites;
            this.iso = iso;
            Enabled = true;
            comboBox1.BeginUpdate();
            comboBox1.Items.Clear();
            for (int i = 0; i < allSprites.Count; i++)
            {
                comboBox1.Items.Add(Sprites[i]);
            }
            comboBox1.SelectedIndex = 1;
            comboBox1.EndUpdate();

            int j = 0;
            JsonSpriteAttributes json = new JsonSpriteAttributes();
            
            foreach (var value in comboBox1.Items)
            {

                JsonSpriteAttribute attribute = new JsonSpriteAttribute();
                //var CharacterSprite spr = new CharacterSprite();
                if (value is CharacterSprite)
                {
                    CharacterSprite spr = (CharacterSprite) value;
                    attribute.index = j;
                    attribute.name = spr.ToString();
                    attribute.flying = spr.Flying;
                    attribute.SHP = spr.SHP.ToString();
                    attribute.SEQ = spr.SEQ.ToString();
                }else if (value is WepSprite)
                {
                    WepSprite spr = (WepSprite)value;
                    attribute.index = j;
                    attribute.name = spr.ToString();
                    attribute.flying = false;
                    attribute.SHP = "WEP";
                    attribute.SEQ = "WEP";
                }
                else
                {
                    CharacterSprite spr = (CharacterSprite)value;
                    attribute.index = j;
                    attribute.name = spr.ToString();
                    attribute.flying = false;
                    attribute.SHP = spr.SHP.ToString();
                    attribute.SEQ = spr.SEQ.ToString();
                }


                j++;
                json.attributes.Add(attribute);
            }


            JsonSerializer serializer = new JsonSerializer();
            using (StreamWriter sw = new StreamWriter(@"C:\Users\acurr\Documents\FFT\for_grudenheim.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, json);
            }

        }

        private class JsonSpriteAttributes
        {

            public List<JsonSpriteAttribute> attributes;


            public JsonSpriteAttributes()
            {
                attributes = new List<JsonSpriteAttribute>();
            }
        }

        private class JsonSpriteAttribute
        {
            public string name;
            public int index;
            public int palette_index;
            public string SEQ;
            public string SHP;
            public bool flying;
        }

        public AllSpritesEditor()
        {
            InitializeComponent();
            Enabled = false;

            spriteEditor1.SpriteDragEnter += sprite_DragEnter;
            spriteEditor1.SpriteDragDrop += sprite_DragDrop;
        }

        public void ReloadCurrentSprite(bool updateAnimationTab = true)
        {
            if (CurrentSprite != null)
            {
                spriteEditor1.ReloadSprite(updateAnimationTab);
            }
        }

        public event DragEventHandler SpriteDragEnter;
        protected virtual void OnSpriteDragEnter(DragEventArgs e)
        {
            DragEventHandler handler = SpriteDragEnter;
            if (handler != null)
                handler(this, e);
        }
        private void sprite_DragEnter(object sender, DragEventArgs e)
        {
            OnSpriteDragEnter(e);
        }

        public event DragEventHandler SpriteDragDrop;
        protected virtual void OnSpriteDragDrop(DragEventArgs e)
        {
            DragEventHandler handler = SpriteDragDrop;
            if (handler != null)
                handler(this, e);
        }
        private void sprite_DragDrop(object sender, DragEventArgs e)
        {
            OnSpriteDragDrop(e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sprite sprite = (sender as System.Windows.Forms.ComboBox).SelectedItem as Sprite;
            if (sprite != null)
            {
                spriteEditor1.BindTo(sprite, Sprites.SharedSPRs[sprite], iso);
                string text = comboBox1.SelectedItem.ToString();
                bool useGenericNames = ((text == "WEP1") || (text == "WEP2") || (text == "EFF1") || (text == "EFF2") || (text == "TRAP1"));
                spriteEditor1.UpdatePaletteDropdownNames(useGenericNames);
            }
        }
    }
}
