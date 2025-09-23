import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { WebAssemblyService } from './services/webassembly.service';

@Component({
	standalone: true,
	selector: 'app-root',
	imports: [RouterOutlet, FormsModule, CommonModule],
	schemas: [CUSTOM_ELEMENTS_SCHEMA],
	templateUrl: './app.component.html',
	styleUrl: './app.component.scss'
})
export class AppComponent {
	constructor(public webAssemblyService: WebAssemblyService) {
	}

	async ngOnInit() {
		await this.webAssemblyService.initialize('/wasm/v1/wasm-loader.js');
	}

	errorInThreads() {
		this.webAssemblyService.testManager?.errorInThreads();
	}
}
