import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
	providedIn: 'root'
})
export class WebAssemblyService {

	private assemblyExports: any;
	public testManager: TestManager | undefined;

	private initialized = new BehaviorSubject<boolean>(false);
	public initialized$ = this.initialized.asObservable();

	public async initialize(url: string): Promise<void> {
		if (this.assemblyExports) return; // Prevent reloading

		const webAssembly = await import(/* @vite-ignore */url);

		this.assemblyExports = webAssembly.assemblyExports;
		this.testManager = new TestManager(this.assemblyExports.wasmbrowser.TestManager);

		this.initialized.next(true);
	}
}

class TestManager {
	constructor(private testManager: any) {
	}
	public errorInThreads(): void {
		return this.testManager.ErrorInThreads();
	}
	public errorInRunTime(): void {
		return this.testManager.ErrorInRuntime();
	}
}
