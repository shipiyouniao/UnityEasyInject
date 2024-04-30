# Unity Easy Inject

## Table of Contents

* [Introduction](#introduction)
* [Why Choose Unity Easy Inject?](#why-choose-unity-easy-inject)
* [Installation](#installation)
* [Usage](#usage)
    * [Start the IoC Container](#1-start-the-ioc-container)
    * [Non-GameObject Component Class Object](#2-non-gameobject-component-class-object)
        * [Register Object](#21-register-object)
        * [Field or Property Injection to Get Bean](#22-field-or-property-injection-to-get-bean)
        * [Constructor Injection to Get Bean](#23-constructor-injection-to-get-bean)
        * [Bean Name](#24-bean-name)
        * [Non-GameObject Component Class Bean Based on Liskov Substitution Principle](#25-non-gameobject-component-class-bean-based-on-liskov-substitution-principle)
    * [GameObject Object](#3-gameobject-object)
        * [Register GameObject Component Class Which Already Exists in the Scene](#31-register-gameobject-component-class-which-already-exists-in-the-scene)
        * [Bean Name](#32-bean-name)
        * [Register GameObjects Without Writing GameObject Component Classes](#33-register-gameobjects-without-writing-gameobject-component-classes)
        * [Add a New GameObject as a Bean to the Scene](#34-add-a-new-gameobject-as-a-bean-to-the-scene)
        * [GameObject Component Class Bean Based on Liskov Substitution Principle](#35-gameobject-component-class-bean-based-on-liskov-substitution-principle)
        * [GameObject Component Class Bean Across Scenes](#36-gameobject-component-class-bean-across-scenes)
        * [Delete GameObject Component Class Bean](#37-delete-gameobject-component-class-bean)
* [Future Plans](#future-plans)
* [Contact Information](#contact-information)

---

## Introduction

Unity Easy Inject is a Unity dependency injection (DI) framework that can help you better manage dependencies in Unity
projects, making projects easier to maintain and expand.

Using this framework, you can replace the way of manually adding public fields and then dragging and dropping injections
in the Inspector for reference, or replace the way of declaring interface classes and then instantiating implementation
classes, reducing module coupling and making projects easier to maintain and expand.

The usage of this framework is inspired by Spring Boot, so the usage is very similar to it.

However, since the project is still in its early stages, only class objects can be registered as Beans.

The project is developed by a junior of a college who has shifted from WEB to Unity as a newcomer, so there may be some
shortcomings. Suggestions are welcome.

---

## Why Choose Unity Easy Inject?

* **Simple and Easy to Use**: With just a few lines of code, you can achieve dependency injection, simplifying the
  development process.
* **Based on Attributes**: Use attributes to register Beans, no need for additional configuration files.
* **Low Coupling**: Using dependency injection can reduce the coupling between components, making the project easier to
  maintain and expand.

When developing projects with Unity, we often encounter the following problem: when a game component needs to use
another game component, we need to add a field with the `public` modifier, and then manually drag and drop the other
component to this field in the Unity editor.

Although this approach is simple, as the project grows larger, this approach becomes more and more cumbersome, and the
coupling also becomes higher and higher.

At this point, we will look for a better solution, Inversion of Control (IoC) is one of them.

If you have used dependency injection frameworks such as Zenject, you will find that we need to manually register class
objects as Beans in the container, which makes the project more complex, such as this:

```csharp
public class TestInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<TestComponent>().AsSingle();
    }
}
```

By using Unity Easy Inject, you just need to add a few attributes to the class, and the class will be registered as a
Bean, which is much simpler and easier to use.

It is easy to inject dependencies into the class. Just add an attribute to the field with the `private` modifier, and
the dependency will be injected automatically.

You do not need to write any configuration files, and you do not need to manually register the class as a Bean in the
container, such as this:

```csharp
[GameObjectBean]
public class TestMonoBehaviour : MonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    private void Awake()
    {
        testComponent.SetActive(true);
    }
}
```

Can not wait to try it? Let's get started!

---

## Installation

### 1. Download the Project

You can download the project from the [GitHub repository](https://github.com/shipiyouniao/UnityEasyInject/tree/main) by
clicking the green Code button on the GitHub repository page and selecting Download ZIP.

Just unzip the downloaded file at `Assets` directory of your Unity project, and you are ready to go.

### 2. Import the Project

You can download the latest Unity Package file (*.unitypackage) from the Releases page on the repository page.

Then in Unity, select `Assets` -> `Import Package` -> `Custom Package...`, and select the downloaded Unity Package file.

---

## Usage

### 1. Start the IoC Container

We suggest you to use `GlobalInitializer` in the `EasyInject/Controllers` directory as the startup controller and mount it on the
startup object in each scene.

If the startup time of the startup controller is incorrect, causing the IoC container to not start, please set the
parameter of the DefaultExecutionOrder attribute to a lower number.

```csharp
// Ensure that this script is executed first by setting a very low number
[DefaultExecutionOrder(-1000000)] 
public class GlobalInitializer : MonoBehaviour
{
    public static readonly IIoC Instance = new MyIoC();

    private void Awake()
    {
        Instance.Init();
    }
}
```

The IoC container provides six methods:

* `Init()`: Initialize the IoC container.
* `GetBean<T>(string name = "")`: Get a Bean by name, if the name is not specified, an empty string will be used.
* `CreateGameObjectAsBean<T>(...)`：Create a GameObject as a Bean, which is similar to the `Instantiate` method.
* `DeleteGameObjBean<T>(T bean, string beanName = "", bool deleteGameObj = false, float t = 0.0F)`: Delete a GameObject
  which is a Bean, which is similar to the `Destroy` method.
* `DeleteGameObjBeanImmediate<T>(T bean, string beanName = "", bool deleteGameObj = false)`：Delete a GameObject which is
  a Bean immediately, which is similar to the `DestroyImmediate` method.
* `ClearBeans(...)`: Clear the Beans in the corresponding scene.

***There is no need to use the `ClearBeans` method at the `OnDestroy` method of the `GlobalInitializer` script, as
the `Init` method will automatically clear the Beans in the last scene.***

### 2. Non-GameObject Component Class Object

#### 2.1 Register Object

Non-GameObject component class objects will be registered first and will not be destroyed until the game is closed,
which means you do not need to use `new` to create an instance of the object.

Please use attributes to mark the class as a Bean. Currently, only the `[Component]` feature is available for
registration.

```csharp
[Component]
public class TestComponent
{
    public void Test()
    {
        Debug.Log("TestComponent");
    }
}
```

#### 2.2 Field or Property Injection to Get Bean

You can use the `[Autowired]` attribute to inject the Bean into the field or property where you need to use it.

The injected class must also have the `[Component]` or `[GameObjectBean]` attribute, or any GameObject component class
that is generated as a Bean during the game.

```csharp
[Component]
public class TestComponent2
{
    [Autowired]
    private TestComponent testComponent;
    
    [Autowired]
    public TestComponent testComponent2 { get; set; }

    public void Test()
    {
        testComponent.Test();
        testComponent2.Test();
    }
}
```

#### 2.3 Constructor Injection to Get Bean

You can also use constructor injection to get the Bean.

***Note:*** The constructor injection method is not recommended for GameObject component classes, as the Unity engine
will not be able to instantiate the class.

```csharp
[Component]
public class TestComponent3
{
    private TestComponent testComponent;

    public TestComponent3(TestComponent testComponent)
    {
        this.testComponent = testComponent;
    }

    public void Test()
    {
        testComponent.Test();
    }
}
```

#### 2.4 Bean Name

You can use the `name` parameter of the `[Component]` attribute to specify the name of the Bean.

Then you can use the `[Autowired]` attribute to inject the Bean by name.

This is a good way to make beans unique if they have the same parent class or interface. (Except for `object` or classes
in the `UnityEngine` namespace)

```csharp
[Component(name: "TestComponent4")]
public class TestComponent4
{
    public void Test()
    {
        Debug.Log("TestComponent4");
    }
}

// Inject by using the constructor
[Component]
public class TestComponent5
{
    private TestComponent4 testComponent4;

    public TestComponent5([Autowired("TestComponent4")] TestComponent4 testComponent4)
    {
        this.testComponent4 = testComponent4;
    }

    public void Test()
    {
        testComponent4.Test();
    }
}

// Inject by using the field
[Component]
public class TestComponent6
{
    [Autowired("TestComponent4")]
    private TestComponent4 testComponent4;

    public void Test()
    {
        testComponent4.Test();
    }
}
```

#### 2.5 Non-GameObject Component Class Bean Based on Liskov Substitution Principle

If a class inherits another class or implements an interface, the parent class or interface and the parent class's
parent class and interface (and so on, except for `object` or classes in the `UnityEngine` namespace) will also be
registered as the corresponding information of the Bean instance.

***If the parent class or interface has multiple subclasses or implementation classes, please make sure to use
the `[Component]` attribute to specify a name to make it unique.***

```csharp
public interface ITestService
{
    void Test();
}

[Component]
public class TestService : ITestService
{
    public void Test()
    {
        Debug.Log("TestService");
    }
}

[Component]
public class TestController
{
    private ITestService testService;

    public TestController(ITestService testService)
    {
        this.testService = testService;
    }

    public void Test()
    {
        testService.Test();
    }
}
```

### 3. GameObject Object

#### 3.1 Register GameObject Component Class Which Already Exists in the Scene

You can use the `[GameObjectBean]` attribute to register GameObject component classes that already exist in the scene.

The time of registration is before the `Awake` method is called, so you can use the injected fields in the `Awake`
method.

You cannot use the constructor injection method to inject GameObject component classes, as the Unity engine will not be
able to instantiate the class.

```csharp
[GameObjectBean]
public class TestMonoBehaviour : MonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    private void Awake()
    {
        testComponent.Test();
    }
}
```

```csharp
[GameObjectBean]
public class TestMonoBehaviour2 : MonoBehaviour
{
    [Autowired]
    private TestMonoBehaviour testMonoBehaviour;
    
    private void Awake()
    {
        testMonoBehaviour.gameObject.SetActive(true);
    }
}
```

#### 3.2 Bean Name

If you need to set a name for the GameObject component class, please pass in the name in the `[GameObjectBean]`
attribute.

```csharp
[GameObjectBean("TestMonoBehaviour3")]
public class TestMonoBehaviour3 : MonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    private void Awake()
    {
        testComponent.Test();
    }
}
```

Another way to inject the Bean by name is to use the `ENameType` enumeration type.

* `Custom`: Use the custom name, which is the default value. You do not need to specify this value. This selection is
  usually used when the instance of the class is unique.
* `ClassName`: Use the class name as the name of the Bean. Although this selection is usually used when the instance of
  the parent class is Bean, we still do not recommend using it to make the Bean unique.
* `GameObjectName`: Use the name of the GameObject as the name of the Bean. This selection is usually used when the
  script is attached to a few GameObjects at the same time, which is a good way to make the Bean unique.

```csharp
[GameObjectBean(ENameType.GameObjectName)]
public class TestGameObj : MonoBehaviour
{
    [Autowired]
    private TestComponent testComponent;

    private void Awake()
    {
        testComponent.Test();
    }
}

[GameObjectBean]
public class TestMonoBehaviour3 : MonoBehaviour
{
    // Assume that the name of the GameObject is "TestGameObj"
    [Autowired("TestGameObj")]
    private TestGameObj testGameObj;

    private void Awake()
    {
        testGameObj.gameObject.SetActive(true);
    }
}
```

#### 3.3 Register GameObjects Without Writing GameObject Component Classes

If you want to register GameObjects that do not have GameObject component classes written, you can attach
the `EasyInject/Behaviours/BeanObject` script to the GameObject.

This script will register the object name as a Bean, so you need to pass in the name in the `[Autowired]` attribute when
injecting the field.

***Please ensure that the object name is not duplicated, otherwise unpredictable errors may occur.***

```csharp
[GameObjectBean]
public class TestMonoBehaviour4 : MonoBehaviour
{
    // The name here is the name of the object
    [Autowired("TestObject")]
    private BeanObject testObject;

    private void Awake()
    {
        testObject.SetActive(true);
    }
}
```

#### 3.4 Add a New GameObject as a Bean to the Scene

If you want to add a GameObject as a Bean to the scene, which is not already in the scene, you can use
the `CreateGameObjectAsBean<T>(GameObject original, string beanName, ...)` method provided by the container.

There are many overloads of the method, you can choose the one that suits you best:

`CreateGameObjectAsBean<T>(GameObject original, string beanName)`

`CreateGameObjectAsBean<T>(GameObject original, string beanName, Transform parent)`

`CreateGameObjectAsBean<T>(GameObject original, string beanName, Transform parent, bool instantiateInWorldSpace)`

`CreateGameObjectAsBean<T>(GameObject original, string beanName, Vector3 position, Quaternion rotation)`

`CreateGameObjectAsBean<T>(GameObject original, string beanName, Vector3 position, Quaternion rotation, Transform parent)`

The method is quite different from the `Instantiate(T original, ...)` method, the first parameter is a `GameObject`
prototype, not a generic class `T`. The name of bean is required as the third parameter.

Then you need to pass a type of the component class to the method, and the method will return the instance of the
component class as the type of bean, which is different from the `Instantiate` method.

If you have written a component to the GameObject, it is no need to use `[GameObjectBean]` attribute to mark the class.

***Please ensure that the component class is attached to the GameObject, unless the generic parameter you passed in
is `BeanObject` or `AcrossScenesBeanObject`, the container will automatically attach it for you, otherwise unpredictable
errors may occur.***

***Please check the [GameObject Component Class Bean Across Scenes](#36-gameobject-component-class-bean-across-scenes)
for more information about the `AcrossScenesBeanObject`.***

The chart below shows the parameters of the method:

| Parameter               | Type       | Description                                           |
|-------------------------|------------|-------------------------------------------------------|
| original                | GameObject | The prototype of the GameObject.                      |
| beanName                | string     | The name of the Bean.                                 |
| parent                  | Transform  | The parent of the GameObject.                         |
| instantiateInWorldSpace | bool       | Whether to instantiate the GameObject in world space. |
| position                | Vector3    | The position of the GameObject.                       |
| rotation                | Quaternion | The rotation of the GameObject.                       |

```csharp
[GameObjectBean]
public class TestMonoBehaviour5 : MonoBehaviour
{
    public GameObject prefab;
    
    private void Start()
    {
        // Create a new GameObject as a Bean
        var go = GlobalInitializer.Instance.CreateGameObjectAsBean<BeanObject>(prefab, "testObj", transform);
        go.SetActive(true);
    }
}
```

#### 3.5 GameObject Component Class Bean Based on Liskov Substitution Principle

GameObject component classes are also based on the Liskov Substitution Principle. (Except for `object` or classes in
the `UnityEngine` namespace)

***If the parent class or interface has multiple subclasses or implementation classes, please make sure to specify a
name to make it unique.***

```csharp
public interface ISay
{
    void say();
}

[GameObjectBean("TestMonoBehaviour6")]
public class TestMonoBehaviour6 : MonoBehaviour, ISay
{
    public void say()
    {
        Debug.Log("TestMonoBehaviour6");
    }
}

[GameObjectBean("TestMonoBehaviour7")]
public class TestMonoBehaviour7 : MonoBehaviour, ISay
{
    public void say()
    {
        Debug.Log("TestMonoBehaviour7");
    }
}

[GameObjectBean]
public class TestMonoBehaviour8 : MonoBehaviour
{
    [Autowired("TestMonoBehaviour6")]
    private ISay testMonoBehaviour6;
    
    [Autowired("TestMonoBehaviour7")]
    private ISay testMonoBehaviour7;
    
    private void Awake()
    {
        testMonoBehaviour6.say();
        testMonoBehaviour7.say();
    }
}
```

#### 3.6 GameObject Component Class Bean Across Scenes

If you need to register a Bean across scenes, you can use the `[PersistAcrossScenes]` attribute. Please ensure that the
class calls `DontDestroyOnLoad()` during initialization.

If it is no need to write any component class, you can attach the `EasyInject/Behaviours/AcrossScenesBeanObject` script
to the GameObject. The script is a subclass of `BeanObject` and will automatically attach the `PersistAcrossScenes`
attribute.

```csharp
[PersistAcrossScenes]
[GameObjectBean]
public class TestAcrossScenes : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
```

#### 3.7 Delete GameObject Component Class Bean

If you need to delete a GameObject component class Bean, do not use the `Destroy` method directly, as the Bean will not
be deleted from the container.

You can use the `DeleteGameObjBean<T>(T bean, string beanName = "", bool deleteGameObj = false, float t = 0.0F)` method
provided by the container.

The method is quite similar to the `Destroy` method. `bean` is the instance of the component class, `beanName` is the
name of the Bean, `deleteGameObj` is whether to delete the GameObject, and `t` is the delay time.

The container also provides
the `DeleteGameObjBeanImmediate<T>(T bean, string beanName = "", bool deleteGameObj = false)` method, which is quite
similar to the `DestroyImmediate` method. But we do not recommend using it, as it may reduce the performance of the
game.

```csharp
[GameObjectBean]
public class TestMonoBehaviour9 : MonoBehaviour
{
    private void Start()
    {
        // Delete the Bean
        GlobalInitializer.Instance.DeleteGameObjBean(this, "", true);
    }
}
```

If you want to delete the Beans in the corresponding scene, you can use the `ClearBeans(...)` method provided by the container.

There are many overloads of the method, you can choose the one that suits you best:

`ClearBeans(string scene = null, bool clearAcrossScenesBeans = false)`

`ClearBeans(bool clearAcrossScenesBeans)`

`scene` is the name of the scene, `clearAcrossScenesBeans` is whether to clear the Beans across scenes(Which means the gameObject will also be destroyed).

---

## Future Plans

1. Support for more features to make the framework more like Spring Boot while still conforming to Unity.
2. Optimize the logic of initializing IoC containers during scene switching.

---

## Contact Information

If you have any questions or suggestions, or if you would like to contribute to the project, please contact me at the
following email address:

* QQ: 2960474346
* Email: 2960474346@qq.com