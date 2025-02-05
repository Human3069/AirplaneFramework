1. BaseObjectPoolManager : It contains ObjectPoolManager, ObjectPoolHandler class Which is Abstracted,
	- ObjectPoolManager : Handles multiple ObjectPoolHandler classes by Dictionary.
	- ObjectPoolHandler : Executes objects with Queue.

2. BaseObjectPoolController : It contains single ObjectPoolController class Which is Abstracted,
	- ObjectPoolController : Controls and Executes objects.

Between BaseObjectPoolManager and BaseObjectPoolController differences is Count of Queue.
BaseObjectPoolManager Enables Pooling Multiple Objects But BaseObjectPoolController isn't.
So the BaseObjectPoolController is designed to depend on other classes, such as SoundManager and UIManager