﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.IO;
using Botv2.Utilities.IO.FileFormats.BSP;
using Jitter;
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;
using Microsoft.Xna.Framework.Graphics;

/*
	ALL OF THIS CODE SUCKS AND DOESNT WORK THE WAY I WANT IT TO!!!
*/

namespace Botv2
{
	public class Collision
	{
		static World world;
		static public void test()
		{
			if (world == null) loadWorld();


			var rot = Botv2.Utilities.GameHelper.getCameraAngle();
			var pos = Botv2.Utilities.GameHelper.getCameraWorldPosition();
			Vector3 target = new Vector3((float)(Math.Cos(MathHelper.ToRadians(-rot.y)) * Math.Cos(MathHelper.ToRadians(rot.x))), (float)(Math.Sin(MathHelper.ToRadians(rot.x)) * Math.Cos(MathHelper.ToRadians(-rot.y))), (float)Math.Sin(MathHelper.ToRadians(-rot.y)));

			JVector origin = new JVector(pos.x, pos.y, pos.z);
			JVector dest = origin - new JVector(target.X, target.Y, target.Z);

			JVector temp1 = new JVector(0,0,0);
			float temp2 = 0;
			RigidBody hitBody;

			world.CollisionSystem.Raycast(origin, dest, null, out hitBody, out temp1, out temp2);
			if (hitBody != null)
			{
				Console.WriteLine("Apparently I hit something");
				Console.WriteLine(origin + (dest * temp2));
			}
			else Console.WriteLine("Apparently I didn't hit something");

		}

		static public void loadWorld()
		{
			CollisionSystem collision = new CollisionSystemPersistentSAP();
			world = new World(collision);

			string mapName = Botv2.Utilities.GameHelper.getMapName();
			string mapPath = Path.GetDirectoryName(Bot.process.Modules[0].FileName) + "/csgo/maps/" + mapName;

			FileStream stream = File.OpenRead(mapPath);
			BSP map = new BSP(stream);

			List<Vector3> vertices = map.getVertices();
			List<ushort[]> edges = map.getEdges();
			List<Face> faces = map.getOriginalFaces();
			map.getFaces();
			int[] surfedges = map.getSurfedges();
			int[] textureData = map.getTextureInfo();

			stream.Close();

			for (int i = 0; i < faces.Count; i++)
			{	
				Face face = faces[i];
				List<JVector> jVertices = new List<JVector>();
				List<VertexPositionColor> temp = new List<VertexPositionColor>();

				for (int b = 0; b <= face.numEdges; b++)
				{
					if (surfedges[face.firstEdge + b] < 0)
					{
						temp.Add(new VertexPositionColor(vertices[(int)edges[Math.Abs(surfedges[face.firstEdge + b])][0]], Color.LightGray));
						temp.Add(new VertexPositionColor(vertices[(int)edges[Math.Abs(surfedges[face.firstEdge + b])][1]], Color.LightGray));
						continue;
					}

					int edgeIndex = Math.Abs(surfedges[face.firstEdge + b]);
					int verticeOne = edges[edgeIndex][1];
					int verticeTwo = edges[edgeIndex][0];

					temp.Add(new VertexPositionColor(vertices[verticeOne], Color.LightGray));
					temp.Add(new VertexPositionColor(vertices[verticeTwo], Color.LightGray));
				}

				foreach (VertexPositionColor v in temp) jVertices.Add(new JVector(v.Position.X, v.Position.Y, v.Position.Z));
				RigidBody rigBody = new RigidBody(new ConvexHullShape(jVertices));

				world.AddBody(rigBody);
			}
		}
	}
}
