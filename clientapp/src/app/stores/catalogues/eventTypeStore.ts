import { makeAutoObservable, runInAction } from "mobx";
import agent from "../../api/agent";
import { EventType } from "../../models/catalogues/eventType";
import ModuleStore from "../moduleStore";

class EventTypeStore {
    eventTypeRegistry = new Map<string, EventType>();
    selectedElement: EventType | undefined = undefined;
    detailsElement: EventType | undefined = undefined;
    editMode: boolean = false;
    loading: boolean = false;
    loadingInitial: boolean = true;
    typeOptions: { key: string; text: string; value: string; }[] = [];
    typeOptionsLoading: boolean = false;

    constructor() {
        makeAutoObservable(this);
    }

    get getArray() {
        return Array.from(this.eventTypeRegistry.values());
    }

    loadOptions = () => {
        this.typeOptionsLoading = true;
        this.typeOptions = [];

        if (this.getArray.length > 0) {
            this.getArray.forEach(type => {
                const opt = { key: type.id, text: type.type, value: type.id }
                this.typeOptions?.push(opt);
            })
        }

        this.typeOptionsLoading = false;
    }

    clearData = () => {
        this.eventTypeRegistry.clear();
        this.loadingInitial = true;
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    loadList = async () => {
        this.clearData();

        try {
            const result = await agent.EventTypes.list();
            result.forEach(eventType => {
                ModuleStore.convertEntityDateFromApi(eventType);

                this.eventTypeRegistry.set(eventType.id!, eventType);

                this.typeOptions?.push({ key: eventType.id, text: eventType.type, value: eventType.id });
            })
            //this.loadOptions();

        } catch (error) {
            console.log(error);
        } finally {
            this.setLoadingInitial(false);
        }
    }

    selectOne = (id: string) => {
        this.selectedElement = this.eventTypeRegistry.get(id);
    }

    cancelSelectedElement = () => {
        this.selectedElement = undefined;
    }

    getOne = (id: string) => {
        return this.eventTypeRegistry.get(id)!;
    }

    details = async (id: string) => {
        this.detailsElement = await agent.EventTypes.getOne(id);
    }

    createOne = async (eventType: EventType) => {
        this.loading = true;

        try {
            await agent.EventTypes.createOne(eventType);

            runInAction(() => {
                this.eventTypeRegistry.set(eventType.id!, eventType);
                this.selectedElement = eventType;
            });
        } catch (error) {
            console.log(error);
        } finally {
            runInAction(() => {
                this.loading = false;
                this.editMode = false;
            });
        }
    }

    editOne = async (eventType: EventType) => {
        this.loading = true;

        try {
            await agent.EventTypes.editOne(eventType);
            runInAction(() => {
                this.eventTypeRegistry.set(eventType.id!, eventType);
                this.selectedElement = eventType;
                this.editMode = false;
            })
        } catch (error) {
            console.log(error);
        } finally {
            runInAction(() => {
                this.loading = false;
            });
        }
    }

    deleteOne = async (id: string) => {
        this.loading = true;

        try {
            await agent.EventTypes.deleteOne(id);
            runInAction(() => {
                this.eventTypeRegistry.delete(id);
                if (this.selectedElement?.id === id) this.cancelSelectedElement();
            })
        } catch (error) {
            console.log(error);
        } finally {
            runInAction(() => {
                this.loading = false;
            });
        }
    }
}

export default EventTypeStore