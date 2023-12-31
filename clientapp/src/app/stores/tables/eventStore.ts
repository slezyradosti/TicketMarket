import { makeAutoObservable, runInAction } from "mobx";
import agent from "../../api/agent";
import { Event } from "../../models/tables/event";

class EventStore {
    eventRegistry = new Map<string, Event>();
    selectedElement: Event | undefined = undefined;
    editMode: boolean = false;
    loading: boolean = false;
    loadingInitial: boolean = true;

    constructor() {
        makeAutoObservable(this);
    }

    clearData = () => {
        this.eventRegistry.clear();
        this.loadingInitial = true;
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    loadList = async () => {
        try {
            const result = await agent.Events.list();
            result.forEach(event => {
                this.eventRegistry.set(event.id!, event);
            })
        } catch (error) {
            console.log(error);
        } finally {
            this.setLoadingInitial(false);
        }
    }

    loadSellersList = async () => {
        try {
            const result = await agent.Events.sellersList();
            result.forEach(event => {
                // event.createdAt = event.createdAt?.split('T')[0];
                // event.updatedAt = event.updatedAt?.split('T')[0];
                this.eventRegistry.set(event.id!, event);
            })
        } catch (error) {
            console.log(error);
        } finally {
            this.setLoadingInitial(false);
        }
    }

    selectOne = (id: string) => {
        this.selectedElement = this.eventRegistry.get(id);
    }

    cancelSelectedElement = () => {
        this.selectedElement = undefined;
    }

    getOne = (id: string) => {
        return this.eventRegistry.get(id)!;
    }

    details = async (id: string) => {
        return await agent.Events.getSellersOne(id);
    }

    createOne = async (event: Event) => {
        this.loading = true;

        try {
            await agent.Events.createOne(event);

            runInAction(() => {
                this.eventRegistry.set(event.id!, event);
                this.selectedElement = event;
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

    editOne = async (event: Event) => {
        this.loading = true;

        try {
            await agent.Events.editOne(event);
            runInAction(() => {
                this.eventRegistry.set(event.id!, event);
                this.selectedElement = event;
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
            await agent.Events.deleteOne(id);
            runInAction(() => {
                this.eventRegistry.delete(id);
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

export default EventStore