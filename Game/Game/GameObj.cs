using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.Diagnostics;


namespace Game
{
    //helpers
    //TODO: Issue #22: delete Edgez and implement new collision detection.
    public enum Edgez { Top, Bottom, Left, Right, Hover };//Hover means the particle is insanely close to the object, but it should hover instead of moving away
    struct Animation
    {
        public string name;
        public byte beginX;
        public byte beginY;
        public byte length;
        public bool loop;
        public byte time;
    }
    class ExitVector
    {
        public Vector2 vector { get; set; }
        public Edgez edge { get; set; }
        public ExitVector(Vector2 v, Edgez e) { vector = v; edge = e; }
    }

    /// <summary>
    /// This class maintains a list of all the Things of various descriptions on the stage.
    /// It has functions for batch jobs (like collision detection), as well as wrappers for List<> functions
    /// </summary>    
    class ThingHandler : List<Thing>
    {

        public SpriteBatch spriteBatch;
        /// <summary>
        /// Instantiates a ThingHandler.
        /// A ThingHandler groups Things together for the purposes of collision detection and ease of Update()ing and Draw()ing
        /// </summary>
        /// <param name="s">A SpriteBatch to pass to Things that need it</param>
        public ThingHandler(SpriteBatch s)
        {
            //accept a SpriteBatch to send to Things' drawing functions
            spriteBatch = s;
        }
        /// <summary>
        /// Adds a thing to the ThingHandler for controlling
        /// </summary>
        /// <param name="Th">the Thing to be added</param>
        /// <returns>An item index of the Thing added</returns>
        public new int Add(Thing Th)
        {
            base.Add(Th);
            return base.LastIndexOf(Th);
        }

        /// <summary>
        /// Update all Things in list
        /// (calls the Update function of all Things)
        /// </summary>
        public void Update()
        {
            foreach (Thing t in this)
            {

                if (t is Sentient)
                {
                    t.Update(this); //the player override expects a ThingHandler to check collisions
                }
                else
                {
                    t.Update();
                }
            }
        }

        /// <summary>
        /// Redraw all Things in list
        /// (calls the Draw function of all Things)
        /// </summary>
        public void Draw()
        {
            spriteBatch.Begin();
            foreach (Thing t in this)
            {
                t.Draw(spriteBatch);
                Algebra.DrawLine(spriteBatch, new Vector2(t.AABB.Center.X, t.AABB.Center.Y), t.velocity);
            }
            spriteBatch.End();
        }

        /// <summary>
        /// This function returns a List containing every thing in this ThingHandler that is intersecting the given Thing
        /// </summary>
        /// <param name="t">The Thing to be tested</param>
        /// <returns>A list of Things that might be intersecting the passed Thing</returns>
        public List<Thing> Intersectors(Thing t, out Vector2 exit)
        {
            List<Thing> InterThings = new List<Thing> { };
            exit = new Vector2();
            foreach (Thing th in this)
            {
                if (th != t)
                {
                    if (th.AABB.Intersects(new Rectangle((int)t.location.X,(int)t.location.Y,t.AABB.Width,t.AABB.Height)))//rough AABB collisions
                    {
                        List<Vector2> tPolyPos = new List<Vector2>();
                        List<Vector2> thPolyPos = new List<Vector2>();
                        foreach (Vector2 v in t.polygonBB)
                        {
                            tPolyPos.Add(v + t.location);
                        }
                        foreach (Vector2 v in th.polygonBB)
                        {
                            thPolyPos.Add(v + th.location);
                        }
                        Vector2 exitVec = Algebra.Intersects(tPolyPos, thPolyPos);//fine polygon collisions
                        if (exitVec.X!=0 && exitVec.Y!=0)
                        {
                            //probably something wrong here
                            exit += exitVec;
                            InterThings.Add(th);
                        }
                    }
                }
            }
            return InterThings;
        }
        /// <summary>
        /// Decides which exit velocity vector a particle should have after hitting a deflector.
        /// </summary>
        /// <param name="particle">The Thing which will bounce</param>
        /// <param name="deflector">The Thing that remains stationary</param>
        /// <returns>
        /// An ExitVector class containing the velocity vector and the edge which has been intersected.
        /// Apply this vector to the particle.
        /// </returns>
        // TODO: Issue #22: Totally change collision detection to be more precise. 
        //       - Define polygons for all objects, and use dividing axis method to solve collisions, if Intersecting() says the objects are close enough (for optimisation)
        public ExitVector Bounce(Thing particle, Thing deflector)
        {
            return new ExitVector(new Vector2(), new Edgez());
        }


    }

    /********************************************************************************************************************
     *                                                                                                                  *
     *                                                                                                                  *
     *                                                                                                                  *
     *                                                                                                                  *
     *                                                                                                                  *
     *                                            THIS PAGE INTENTIONALLY LEFT BLANK                                    *
     *                                                                                                                  *
     *                                                                                                                  *
     *                                                                                                                  *
     *                                                                                                                  *
     *                                                                                                                  *
     ********************************************************************************************************************/
    class Thing
    {
        protected XmlTextReader XMLAsset;
        // STATIC PROPERTIES OF THING:
        private Texture2D Texture;

        public float gravity=0;  // amount of gravity to pull. Try -7f
          //animation
          protected int frameWidth, frameHeight;
          protected List<Animation> animations=new List<Animation>();
          protected byte x=255,y=255;
        // CHANGING PROPERTIES
        public Vector2 location; //position in space
          //animation
          Animation curAnim;
          protected byte frameCurrent = 0;
          protected double lastTime = 0;
          protected long period = 0;
          protected long nextChangeTime = 0;
        //NOT SURE
        public List<Vector2> polygonBB=new List<Vector2>(); //precise bounds of Thing

        public Rectangle AABB; //Rough box around the Thing

        public Vector2 velocity = new Vector2(0, 0); //velocity vector

        
        // Main functions of class
        /// <summary>
        ///  Automatic class constructor for "things" on the screen - blocks, bots, characters, etc...
        /// </summary>
        /// <param name="XMLAssetName">The name of the XML file that describes this object</param>
        public Thing(string XMLAssetName, ContentManager Content)
        {
            XMLAsset = new XmlTextReader(string.Format("Content/{0}.xml", XMLAssetName));
            while (XMLAsset.Read())
            {
                if (XMLAsset.NodeType == XmlNodeType.Element)
                {
                    if (XMLAsset.Name == "Thing") { break; }
                }
            }
            while (XMLAsset.Read())
            {
                if (XMLAsset.NodeType == XmlNodeType.Element)
                {
                    //Apply elements to the object, with error checking
                    if (XMLAsset.Name == "Texture")
                    {
                        while (XMLAsset.MoveToNextAttribute())
                        {
                            if (XMLAsset.Name == "x") { x = byte.Parse(XMLAsset.Value); continue; }
                            if (XMLAsset.Name == "y") { y = byte.Parse(XMLAsset.Value); continue; }
                        }
                        if (x == 255 || y == 255) { throw (new Exception("x and y values of texture are required")); }

                        XMLAsset.Read();
                        if (XMLAsset.NodeType == XmlNodeType.EndElement) { throw (new Exception("Texture is a required node")); }
                        else if (XMLAsset.NodeType == XmlNodeType.Text) { Texture = Content.Load<Texture2D>(XMLAsset.Value); }
                        frameHeight = Texture.Height / y; //figure out how big frames are in this texture.
                        frameWidth = Texture.Width / x;
                        continue;
                    }
                    if (XMLAsset.Name == "Gravity")
                    {
                        XMLAsset.Read();
                        if (XMLAsset.NodeType == XmlNodeType.Text)
                        {
                            gravity = float.Parse(XMLAsset.Value);
                        }
                        continue;
                    }
                    if (XMLAsset.Name == "Animation")
                    {
                        string name = "";
                        byte beginX = 255, beginY = 255, length = 255, time = 255;
                        bool? loop = null;
                        while (XMLAsset.MoveToNextAttribute())
                        {
                            if (XMLAsset.Name == "name") { name = XMLAsset.Value; continue; }
                            if (XMLAsset.Name == "beginX") { beginX = byte.Parse(XMLAsset.Value); continue; }
                            if (XMLAsset.Name == "beginY") { beginY = byte.Parse(XMLAsset.Value); continue; }
                            if (XMLAsset.Name == "length") { length = byte.Parse(XMLAsset.Value); continue; }
                            if (XMLAsset.Name == "time") { time = byte.Parse(XMLAsset.Value); continue; }
                            if (XMLAsset.Name == "loop") { loop = bool.Parse(XMLAsset.Value); continue; }
                        }
                        if (name == "" || beginX == 255 || beginY == 255 || length == 255 || time == 255 || loop == null) { throw new Exception("not all parms for Animation given"); }
                        foreach (Animation a in animations)
                        {
                            if (a.name == name)
                            {
                                throw new Exception("Animation names must be unique");
                            }
                        }
                        Animation an = new Animation();
                        an.name = name;
                        an.beginX = beginX;
                        an.beginY = beginY;
                        an.length = length;
                        an.time = time;
                        an.loop = (bool)loop;
                        animations.Add(an);
                        continue;
                    }
                    if (XMLAsset.Name == "Polygon")
                    {
                        // PolygonBB
                        polygonBB.Clear();
                        while (XMLAsset.Read())
                        {
                            if (XMLAsset.Name == "Point" && XMLAsset.NodeType==XmlNodeType.Element)
                            {
                                uint x = uint.MaxValue, y = uint.MaxValue;
                               // XMLAsset.Read();
                                while (XMLAsset.MoveToNextAttribute())
                                {
                                    if (XMLAsset.Name == "x") { x = uint.Parse(XMLAsset.Value); continue; }
                                    if (XMLAsset.Name == "y") { y = uint.Parse(XMLAsset.Value); continue; }
                                }
                                if (x == uint.MaxValue || y == uint.MaxValue) { throw new Exception("Polygon not fully described. Missing x/y value"); }
                                polygonBB.Add(new Vector2(x, y));
                            }
                        }
                        AABB = Algebra.Span(polygonBB);
                    }
                }
            }
            if (polygonBB.Count() < 3) { throw new Exception("A polygon is required."); }
        }
        /// <summary>
        /// Organises game logic for the Thing.
        /// This should usually be called by the ThingHandler.
        /// It's very likely that it'll be overriden by derivative classes
        /// </summary>
        /// <param name="handle">Used by some objects to handle collisions with other objects</param>
        /// <param name="SuppressMove">Deprecated: If true, the object should not move itself this cycle. This is going to be handled by something else once proper collision detection is here.</param>
        public virtual void Update(ThingHandler handle = null, bool SuppressMove = false) 
        {
            //TODO: Issue #22: Figure how this is going to work with new physics. We need to check if we CAN move or not.

            //I think this update routine is called not-as-quickly as draw, but still pretty damn fast
            double time = (double)DateTime.Now.Ticks / 1000000; //a million
            double dT = 0.1/*time - lastTime*/;
            lastTime = time;

                this.Move(new Vector2(this.location.X + (int)(velocity.X * dT), (this.location.Y + (int)(-velocity.Y * dT))));
                this.velocity = new Vector2(this.velocity.X, this.velocity.Y + gravity);

            AABB = new Rectangle((int)this.location.X, (int)this.location.Y, this.frameWidth, this.frameHeight);
            if (nextChangeTime <= DateTime.Now.Ticks)
            {
                nextChangeTime = DateTime.Now.Ticks + period;
                if (frameCurrent - curAnim.beginX + 1 < curAnim.length)
                {
                    frameCurrent++;
                }
                else if (curAnim.loop)
                {
                    frameCurrent = curAnim.beginX;
                }
            }
        }
        /// <summary>
        /// Send an animation to the Animation handler of the Thing.
        /// </summary>
        /// <param name="name">The name of the Animation element in the XML file of the Thing</param>
        public void StartAnimation(string name)
        {
            Animation thisAnim=new Animation();
            foreach (Animation a in animations)
            {
                if (a.name == name) { thisAnim = a; break; }
            }
            
            if (thisAnim.beginX != curAnim.beginX || thisAnim.beginY != curAnim.beginY)
            {
                if (thisAnim.beginY < y)
                {
                    if (thisAnim.beginX + thisAnim.length <= x)
                    {
                        //should be valid sequence
                        curAnim.beginX = thisAnim.beginX;
                        curAnim.beginY = thisAnim.beginY;
                        curAnim.length = thisAnim.length;
                        curAnim.loop = thisAnim.loop;
                        frameCurrent = thisAnim.beginX;
                        curAnim.time=thisAnim.time;

                        nextChangeTime = DateTime.Now.Ticks + curAnim.time * 10000;
                    }
                    else { throw new Exception("Invalid Animation parameters. See Thing::StartAnimation"); }
                }
                else { throw new Exception("Invalid Animation parameters. See Thing::StartAnimation"); }
            }
        }
        public virtual void Draw(SpriteBatch Sprites)
        {
            
            Rectangle SourceFrame = new Rectangle(frameCurrent * frameWidth, curAnim.beginY * frameHeight, frameWidth, frameHeight);
            Sprites.Draw(Texture, AABB, SourceFrame, Color.White);
            
        }

        public void Move(Vector2 loc)
        {
            //for future's sake - Use this rather than Vector2 *.location
            location = loc;
            AABB.Location = new Point((int)location.X,(int)location.Y);
        }
    }

    /********************************
     * DERIVATIVES OF THING CLASS   *
     *     MOSTLY FOR TESTING       *
     ********************************/

    /// <summary>
    /// Platforms (walls) in the game will be of this type.
    /// See Thing for details
    /// </summary>
    class Platform : Thing
    {
        public Platform(string XMLAssetName, ContentManager Content) : base(XMLAssetName, Content) { }
    }

    enum JumpState { Up, Down, Not };
    /// <summary>
    /// Intended for objects that either move because of user input or AI
    /// </summary>
    class Sentient : Thing
    {

        public JumpState jumping = JumpState.Not;

        //remembers the sort of animation that must happen during phases of the jump
        private string DownAnim = "";
        private string LandAnim = "";

        public Sentient(string XMLAssetName, ContentManager Content) : base(XMLAssetName, Content) { }

        public override void Update(ThingHandler things, bool SuppressMove = false)
        {
            // Issue #69: TODO: SERIOUS ADJUSTMENT FOR JUMPING, COLLISON, ETC
            if (this.velocity.Y <= 0 && jumping != JumpState.Not)
            {
                jumping = JumpState.Down;
                
                //StartAnimation(DownStartX, DownStartY, DownLength, DownRepeat, DownTime);
            }
            SuppressMove = false;
            // bounce against intersectors
            Vector2 deflect;
            List <Thing> intThings = things.Intersectors(this, out deflect);
            base.Update(things, SuppressMove);

            if (intThings.Count!=0){
                SuppressMove = true;
               // velocity = Algebra.project(velocity, deflect);
                //TODO: Issue #22: adjust location so object bounces out of wall instead of sticking in it. The line below is wrong!
                this.location += deflect;
            }
            //Add player processing, clamping, etc here

        }
        /// <summary>
        /// Handles a character jumping
        /// </summary>
        /// <param name="UpName">The name of the animation to play while going up</param>
        /// <param name="DownName">The name of the animation to play while going down</param>
        /// <param name="LandName">The name of the animation to play while landing</param>
        public void Jump(string UpName, string DownName, string LandName)
        {
            //TODO: Issue #69: make jumping not suck. Will have to wait for onthegroundness detection
            //TODO: Issue #69: allow for better animation sequences as described in Arthur.xml

                velocity = new Vector2(velocity.X, 100);
                StartAnimation(UpName);

                //remember what animation to play during other phases of jumping
                DownAnim = DownName;
                LandAnim = LandName;

                //remember that we're jumping
                jumping = JumpState.Up;
        }
        public void walk(Vector2 v)
        {
        }
    }
}