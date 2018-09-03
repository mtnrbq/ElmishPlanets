namespace ElmishPlanets

open Urho
open Urho.Actions
open Models
open System

module PlanetVisualizerUrhoApp =
    open Urho.Resources

    let create3DScene (resourceCache: Urho.Resources.ResourceCache) =
        let scene = new Scene()
        scene.LoadXmlFromCache(resourceCache, "Scenes/EarthScene.xml") |> ignore
        scene
        
    let setViewport (renderer: Renderer) (scene: Scene) =
        let cameraNode = scene.GetChild("camera")
        let camera = cameraNode.GetComponent<Camera>()
        let viewport = new Viewport(scene, camera, null)
        renderer.SetViewport(0u, viewport)
        scene
        
    let findPlanet (scene: Scene) =
        scene.GetChild("planet")
        
    let setMaterial (resourceCache: Urho.Resources.ResourceCache) materialPath (node: Node) =
        let staticModel = node.GetComponent<StaticModel>()
        staticModel.SetMaterial(resourceCache.GetMaterial(materialPath))
        node
        
    let setAxialTilt axialTilt (node: Node) =
        node.Rotation <- Quaternion(0.f, 0.f, (float32 axialTilt) * -1.f)
        node
        
    let rotatePlanetForever rotationSpeed (node: Node) =
        let actions: FiniteTimeAction array = [| new RepeatForever(new RotateBy(1.f, 0.f, rotationSpeed, 0.f)) |]
        node.RunActionsAsync(actions)
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.StartImmediate
        
open PlanetVisualizerUrhoApp

type PlanetVisualizerUrhoApp(options: ApplicationOptions) =
    inherit Urho.Application(options)

    member val Scene: Scene = null with get, set

    override this.Start() =
        base.Start()

        let scene =
            create3DScene this.ResourceCache
            |> setViewport this.Renderer

        this.Scene <- scene

    member this.LoadPlanet (planet: Planet) =
        Urho.Application.InvokeOnMain(fun() ->
            let rotationSpeedRelativeToEarth = planet.Info.RotationPeriod / 24.<h>
            let rotationSpeedInDegrees = -22.5 / rotationSpeedRelativeToEarth

            // Round rotation speed for smoother animations
            let rotationSpeed = if rotationSpeedInDegrees > 1. then Math.Round(rotationSpeedInDegrees) else rotationSpeedInDegrees
        
            this.Scene
            |> findPlanet
            |> setMaterial this.ResourceCache ("Materials/" + planet.Info.Name + ".xml")
            |> setAxialTilt (float32 planet.Info.AxialTilt)
            |> rotatePlanetForever (float32 rotationSpeed)
        )