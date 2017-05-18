export interface DataItem {
    id: string;
    creationTimestamp: number;
}
export class Repository<T extends DataItem> {
    private data: Array<T> = [];

    constructor(initial: Array<T>) { this.data = initial.slice(); }

    add(vm: T): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            let id = vm.creationTimestamp.toString();
            this.data.forEach(d => { if (d.id >= id) id = d.id + 1; });
            vm.id = id;
            this.data.push(vm);
            resolve(vm);
        });
    }

    remove(id: string): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            this.data.splice(this.data.findIndex(d => d.id == id), 1);
            resolve();
        });
    }

    getAll(): Promise<Array<T>> {
        return new Promise<Array<T>>((resolve, reject) => {
            resolve(this.data);
        });
    }

    find(id: string): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            resolve(this.data.find(d => d.id == id));
        });
    }

    update(vm: T): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            let oldIndex = this.data.findIndex(d => d.id == vm.id);
            if (oldIndex == -1) reject();
            this.data.splice(oldIndex, 1, vm);
            resolve(this.data[oldIndex]);
        });
    }
}