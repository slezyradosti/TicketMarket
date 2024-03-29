import { makeAutoObservable, runInAction } from "mobx";
import { EventCategory } from "../../models/catalogues/eventCategory";
import agent from "../../api/agent";
import ModuleStore from "../moduleStore";

class EventCategoryStore {
    eventCategoryRegistry = new Map<string, EventCategory>();
    selectedElement: EventCategory | undefined = undefined;
    detailsElement: EventCategory | undefined = undefined;
    editMode: boolean = false;
    loading: boolean = false;
    loadingInitial: boolean = true;
    categoryOptions: { key: string; text: string; value: string; }[] = [];
    categoryOptionsLoading: boolean = false;

    constructor() {
        makeAutoObservable(this);
    }

    get getArray() {
        return Array.from(this.eventCategoryRegistry.values());
    }

    loadOptions = () => {
        this.categoryOptionsLoading = true;
        this.categoryOptions = [];

        if (this.getArray.length > 0) {
            this.getArray.forEach(category => {
                const opt = { key: category.id, text: category.category, value: category.id }
                this.categoryOptions?.push(opt);
            })
        }

        this.categoryOptionsLoading = false;
    }

    clearData = () => {
        this.eventCategoryRegistry.clear();
        this.loadingInitial = true;
    }

    setLoadingInitial = (state: boolean) => {
        this.loadingInitial = state;
    }

    loadList = async () => {
        this.clearData();

        try {
            const result = await agent.EventCategories.list();
            result.forEach(eventCategory => {
                ModuleStore.convertEntityDateFromApi(eventCategory);

                this.eventCategoryRegistry.set(eventCategory.id!, eventCategory);

                this.categoryOptions?.push({ key: eventCategory.id, text: eventCategory.category, value: eventCategory.id })
            })
            //this.loadOptions();

        } catch (error) {
            console.log(error);
        } finally {
            this.setLoadingInitial(false);
        }
    }

    selectOne = (id: string) => {
        this.selectedElement = this.eventCategoryRegistry.get(id);
    }

    cancelSelectedElement = () => {
        this.selectedElement = undefined;
    }

    getOne = (id: string) => {
        return this.eventCategoryRegistry.get(id)!;
    }

    details = async (id: string) => {
        this.detailsElement = await agent.EventCategories.getSellersOne(id);
    }

    createOne = async (eventCategory: EventCategory) => {
        this.loading = true;

        try {
            await agent.EventCategories.createOne(eventCategory);

            runInAction(() => {
                this.eventCategoryRegistry.set(eventCategory.id!, eventCategory);
                this.selectedElement = eventCategory;
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

    editOne = async (eventCategory: EventCategory) => {
        this.loading = true;

        try {
            await agent.EventCategories.editOne(eventCategory);
            runInAction(() => {
                this.eventCategoryRegistry.set(eventCategory.id!, eventCategory);
                this.selectedElement = eventCategory;
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
            await agent.EventCategories.deleteOne(id);
            runInAction(() => {
                this.eventCategoryRegistry.delete(id);
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

export default EventCategoryStore