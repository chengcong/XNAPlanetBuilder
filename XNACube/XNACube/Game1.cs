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

namespace XNACube
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;            
        
        int[] _indicesArray;
        private VertexPositionNormalTexture[] _verticesArray;

        List<int> _indices;
        List<VertexPositionNormalTexture> _vertices;

        SpriteFont sf;
       VertexBuffer vb;
        IndexBuffer ib;        

        private float angle = 0;
        Texture2D heightMap, colorMap, nightMap, reflectionMap, cloudMap, glowMap;
        Matrix World, View, Projection;

        Base3DCamera camera;
        Vector2 lastMouse = new Vector2();

        ClipMap clipMap;

        float rotation = 0;
        float distanceToSurface = 0;
        Vector3 PointOnSphere = Vector3.Zero;
        BasicEffect be;
        VertexPositionColor[] vertices;

        GeometricPrimitive primitive;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1880;
            graphics.PreferredBackBufferHeight = 1040;
            graphics.ApplyChanges();


            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        ///     
        private Effect effect;
        protected override void Initialize()
        {
            Content.RootDirectory = "Content";            
                        
            effect = Content.Load<Effect>("effect");
            be = new BasicEffect(graphics.GraphicsDevice);

            //terrain = new GeoClipMap(this, Content, camera, 255, "ddsTest");

            //effect.AmbientLightColor = Vector3.One;
            //effect.AmbientLightColor = new Vector3(0.0f, 1.0f, 0.0f);
            //effect.DirectionalLight0.Enabled = true;
            //effect.DirectionalLight0.DiffuseColor = Vector3.One;
            //effect.DirectionalLight0.Direction = Vector3.Normalize(Vector3.One);
            //effect.LightingEnabled = false;

            base.Initialize();

            Projection = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4.0f, 
                (float)GraphicsDevice.Viewport.Bounds.Width / (float)GraphicsDevice.Viewport.Bounds.Height,
                0.000001f, 
                20000f);
            //View = Matrix.CreateTranslation(0f, 0f, -1f);

            camera = new Base3DCamera(this, .1f, 2000);
            camera.Position = new Vector3(0, 0, 100);            

            vertices = new VertexPositionColor[4];
            vertices[0] = new VertexPositionColor(Vector3.Zero, Color.Red);
            vertices[1] = new VertexPositionColor(Vector3.Zero, Color.Blue);
            vertices[2] = new VertexPositionColor(Vector3.Zero, Color.Blue);
            vertices[3] = new VertexPositionColor(Vector3.Zero, Color.Red);

            //InitIcoSphere();          
            //InitCubicSphere();
            //InitGeoSphere();

            //CubePrimitive cp = new CubePrimitive(0.5f);
            //CubicSpherePrimitive cp = new CubicSpherePrimitive(10f, 5);
            //int n = 255;
            int n = 31;
            //PlanePrimitive cp = new PlanePrimitive(m, m);
            //primitive = cp;

            clipMap = new ClipMap(n, "hi", Content, this);            

            //GeoSpherePrimitive cp = new GeoSpherePrimitive(0.5f);
            //IcoSpherePrimitive cp = new IcoSpherePrimitive(0.5f, 0);

            
            //InitBuffers(cp.vertices, cp.indices);
        }   

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            heightMap = Content.Load<Texture2D>(@"heightMap");

            Color[] data = new Color[heightMap.Width * heightMap.Height];
            heightMap.GetData<Color>(data);

            heightMap = new Texture2D(GraphicsDevice, heightMap.Width, heightMap.Height, true, SurfaceFormat.Vector4);
            Vector4[] data2 = new Vector4[data.Length];
            for (int x = 0; x < data.Length; x++)
                data2[x] = new Vector4(data[x].R / 255f, data[x].G / 255f, data[x].B / 255f, data[x].A / 255f);
            heightMap.SetData<Vector4>(data2);

            colorMap = Content.Load<Texture2D>(@"colorMap");
            nightMap = Content.Load<Texture2D>(@"glowMap");
            reflectionMap = Content.Load<Texture2D>(@"reflectionMap");
            cloudMap = Content.Load<Texture2D>(@"cloudMap");
            //glowMap = Content.Load<Texture2D>(@"glowMap");

            sf = Content.Load<SpriteFont>(@"font");
        }
            
        private void InitGeoSphere()
        {            
            GeoSpherePrimitive sp = new GeoSpherePrimitive(0.5f);            

            InitBuffers(sp.vertices, sp.indices);
        }

        private void InitIcoSphere()
        {                                    
            IcoSpherePrimitive ics = new IcoSpherePrimitive(0.5f, 3);

            InitBuffers(ics.vertices, ics.indices);
        }


        private void InitBuffers(List<Vector3> Vertices, List<int> Indices)
        {
            _vertices = new List<VertexPositionNormalTexture>();
            _indices = new List<int>();

            /* calculate uv texture mappings */
            foreach (Vector3 v in Vertices)
            {
                Vector3 n = v;
                n.Normalize();

                Vector2 uv = MapUv(n);
                _vertices.Add(new VertexPositionNormalTexture(v, n, uv));
            }
                        
            _indices = Indices;

            /* repair Zig-Zag problem */
            RepairTextureWrapSeam(_vertices, _indices);

            _verticesArray = new VertexPositionNormalTexture[_vertices.Count];
            _indicesArray = new int[_indices.Count];

            /* allocate buffer size */
            vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), _vertices.Count, BufferUsage.WriteOnly);
            ib = new IndexBuffer(GraphicsDevice, typeof(int), _indices.Count, BufferUsage.WriteOnly);

            _vertices.CopyTo(_verticesArray);
            _indices.CopyTo(_indicesArray);

            /* write buffers */
            vb.SetData<VertexPositionNormalTexture>(_verticesArray);
            ib.SetData<int>(_indicesArray);
        }

        private Vector2 MapUv(Vector3 p)
        {
            float u = 0.5f + ((float)Math.Atan2(p.Z, p.X) / MathHelper.TwoPi);
            float v = 0.5f - 2.0f * ((float)Math.Asin(p.Y) / MathHelper.TwoPi);

            return new Vector2(u, v);
        }

        public static void RepairTextureWrapSeam(List<VertexPositionNormalTexture> vertices, List<int> indices)
        {
            var newIndices = new List<int>();

            var corrections = 0;

            /// whenever a vertex is split, add its original and new indices to the dictionary to avoid
            /// creating duplicates.
            var correctionList = new Dictionary<int, int>();

            for (var i = indices.Count - 3; i >= 0; i -= 3)
            {
                /// see if the texture coordinates appear in counter-clockwise order.
                /// If so, the triangle needs to be rectified.
                var v0 = new Vector3(vertices[indices[i + 0]].TextureCoordinate, 0);
                var v1 = new Vector3(vertices[indices[i + 1]].TextureCoordinate, 0);
                var v2 = new Vector3(vertices[indices[i + 2]].TextureCoordinate, 0);

                var cross = Vector3.Cross(v0 - v1, v2 - v1);

                if (cross.Z <= 0)
                {
                    /// this should only happen if the face crosses a texture boundary

                    var corrected = false;

                    for (var j = i; j < i + 3; j++)
                    {
                        var index = indices[j];

                        var vertex = vertices[index];
                        /// 0.9 UV fudge factor - should be able to get rid of this when I get more sleep
                        if (vertex.TextureCoordinate.X >= 0.9f)
                        {
                            /// need to correct this vertex.
                            if (correctionList.ContainsKey(index))
                                newIndices.Add(correctionList[index]);
                            else
                            {
                                var texCoord = vertex.TextureCoordinate;

                                texCoord.X -= 1;
                                vertex.TextureCoordinate = texCoord;
                                corrected = true;

                                vertices.Add(vertex);

                                var correctedVertexIndex = vertices.Count - 1;

                                correctionList.Add(index, correctedVertexIndex);

                                newIndices.Add(correctedVertexIndex);
                            }
                        }
                        else
                            newIndices.Add(index);

                    }

                    if (corrected)
                        corrections++;
                }
                else
                    newIndices.AddRange(indices.GetRange(i, 3));
            }

            //Debug.WriteLine("Corrected {0} of {1}", corrections, newIndices.Count / 3);


            indices.Clear();
            indices.AddRange(newIndices);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private void UpdateInput()
        {
            KeyboardState kState = Keyboard.GetState();            
            
            // Allows the game to exit
            if (kState.IsKeyDown(Keys.Escape))
                this.Exit();

            float speedTran = .1f;            
            float speedRot = .01f;

            if (kState.IsKeyDown(Keys.LeftShift))
                speedTran *= 0.01f;
            if (kState.IsKeyDown(Keys.W))
                camera.Translate(Vector3.Forward * speedTran);
            if (kState.IsKeyDown(Keys.S))
                camera.Translate(Vector3.Backward * speedTran);
            if (kState.IsKeyDown(Keys.A))
                camera.Translate(Vector3.Left * speedTran);
            if (kState.IsKeyDown(Keys.D))
                camera.Translate(Vector3.Right * speedTran);

            if (kState.IsKeyDown(Keys.Left))
                camera.Rotate(Vector3.Up, speedRot);
            if (kState.IsKeyDown(Keys.Right))
                camera.Rotate(Vector3.Up, -speedRot);
            if (kState.IsKeyDown(Keys.Up))
                camera.Rotate(Vector3.Right, speedRot);
            if (kState.IsKeyDown(Keys.Down))
                camera.Rotate(Vector3.Right, -speedRot);

            /* handle mouse movement */
            float leftRightRot = 0, upDownRot = 0;
            if (true)
            {

                if (lastMouse.X != 0 && lastMouse.Y != 0)
                {
                    float mouseX = Mouse.GetState().X - lastMouse.X;
                    float mouseY = Mouse.GetState().Y - lastMouse.Y;

                    if (mouseX != 0)
                        leftRightRot += mouseX * 0.001f;
                    if (mouseY != 0)
                        upDownRot += mouseY * 0.001f;
                }

                int x = GraphicsDevice.Viewport.Width / 2;
                int y = GraphicsDevice.Viewport.Height / 2;

                Mouse.SetPosition(x, y);

                lastMouse.X = x;
                lastMouse.Y = y;
            }

            /* Apply rotation to ship */
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), 0)
                * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot)
                * Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), leftRightRot);

            camera.Rotation *= additionalRot;
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>

        protected override void Update(GameTime gameTime)
        {            
            UpdateInput();

            rotation += 0.01f;

            angle = angle + 0.01f;
            if (angle > 2 * Math.PI) angle = 0;
            Matrix R = Matrix.CreateRotationY(angle) * Matrix.CreateRotationX(0);
            Matrix T = Matrix.CreateTranslation(0.0f, 0f, 0f);
            Matrix S = Matrix.CreateScale(1f);
            
            World = R * T * S;
            
            camera.Update(gameTime);
            View = camera.View;

            clipMap.Update(gameTime);

            //GetSphereIntersection();  

            // TODO: Add your update logic here            
            base.Update(gameTime);
        }

        public void GetSphereIntersection()
        {            
            Vector3 fwd = new Vector3(0, 0, -1);
            Vector3 calc = Vector3.Zero;

            calc += Vector3.Transform(fwd, camera.Rotation);

            vertices[0].Position = calc;            //red
            vertices[1].Position = Vector3.Zero;    //blue

            //calculate point on the sphere
            Vector3 point = Vector3.Zero;
            point += Vector3.Transform(fwd, Matrix.CreateLookAt(Vector3.Zero, camera.Position, new Vector3(0, 1, 0)));

            CubicSpherePrimitive csp = (CubicSpherePrimitive)primitive;

            //what is the closest corner?
            int closestVert = 0;
            float d = float.MaxValue;
            Vector3 closest = Vector3.Zero;
            foreach(Vector3 v in csp.Corners) {
                Vector3 vt = Vector3.Transform(v, World);
                float d1 = Vector3.Distance(camera.Position, vt);
                if (d1 <= d)
                {
                    d = d1;
                    closest = v;
                }
            }

            vertices[2] = vertices[1];
            vertices[3].Position = Vector3.Transform(closest, World);

            //okay got closest "corner"
            //now what? walk?
            //foreach(VertexPositionNormalTexture vpc in _vertices)
            //{

            //}
        }
        
        /// <summary>This is called when the game should draw itself.</summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            RasterizerState rs = new RasterizerState();

            clipMap.Draw(View, Projection);

            #region old testing
            //rs.CullMode = CullMode.CullClockwiseFace;
            //rs.FillMode = FillMode.WireFrame;
            //rs.CullMode = CullMode.None;                

            //GraphicsDevice.SetVertexBuffer(vb);
            //GraphicsDevice.Indices = ib;

            //graphics.GraphicsDevice.RasterizerState = rs;

            //effect.Parameters["MaxHeight"].SetValue(100);
            //effect.Parameters["World"].SetValue(World);
            //effect.Parameters["View"].SetValue(View);
            //effect.Parameters["Projection"].SetValue(Projection);
            //effect.Parameters["LightDirection"].SetValue(Vector3.Normalize(Vector3.One));
            //effect.Parameters["CloudRotation"].SetValue(MathHelper.ToRadians(rotation) * 2.4f);
            //effect.Parameters["CloudHeight"].SetValue(0.03f);
            //effect.Parameters["GlowColor"].SetValue(Color.DodgerBlue.ToVector4());

            //effect.Parameters["heightMap"].SetValue(heightMap);
            //effect.Parameters["ColorMap"].SetValue(colorMap);
            //effect.Parameters["GlowMap"].SetValue(nightMap);
            //effect.Parameters["ReflectionMap"].SetValue(reflectionMap);
            //effect.Parameters["CloudMap"].SetValue(cloudMap);
            //effect.Parameters["GlowMap"].SetValue(nightMap);
            //effect.CurrentTechnique = effect.Techniques["t1"];

            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertices.Count, 0, _indices.Count);
            //}

            //be.View = View;// Matrix.CreateLookAt(camera.Position, (camera.Position + new Vector3(0, 0, -1)), new Vector3(0, 1, 0));
            //be.Projection = Projection;
            //be.World = Matrix.Identity;// R * T * S;
            //be.VertexColorEnabled = true;
            //be.CurrentTechnique.Passes[0].Apply();

            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 2);

            //base.Draw(gameTime);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
            spriteBatch.DrawString(sf,
                string.Format("Cam:{0},{1},{2}",
                    Math.Round((decimal)camera.Position.X, 2),
                    Math.Round((decimal)camera.Position.Y, 2),
                    Math.Round((decimal)camera.Position.Z, 2)),
                Vector2.Zero,
                Color.White);

            //spriteBatch.DrawString(sf,
            //    string.Format("v1:{0},{1},{2}",
            //        Math.Round((decimal)vertices[0].Position.X, 2),
            //        Math.Round((decimal)vertices[0].Position.Y, 2),
            //        Math.Round((decimal)vertices[0].Position.Z, 2)),
            //    Vector2.Zero,
            //    Color.White);

            //spriteBatch.DrawString(sf,
            //    string.Format("v2:{0},{1},{2}",
            //        Math.Round((decimal)vertices[1].Position.X, 2),
            //        Math.Round((decimal)vertices[1].Position.Y, 2),
            //        Math.Round((decimal)vertices[1].Position.Z, 2)),
            //    new Vector2 { X = 0, Y = sf.MeasureString("HI").Y },
            //    Color.White);

            //Vector3 dir = Vector3.Transform(camera.Position, Matrix.CreateFromQuaternion(camera.Rotation));
            //dir.Normalize();

            //spriteBatch.DrawString(sf,
            //    string.Format("d:{0}", Vector3.Distance(vertices[0].Position, vertices[1].Position)),
            //    //string.Format("dir:{0},{1},{2}",
            //    //    dir.X,
            //    //    dir.Y,
            //    //    dir.Z),
            //    new Vector2 { X = 0, Y = sf.MeasureString("HI").Y * 2 },
            //    Color.White);

            spriteBatch.End();
            #endregion

        }        
    }
}
