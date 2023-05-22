declare function init(): void;
declare function joinBetaProgramme(): void;
declare function error(text: string): void;
declare function success(text: string): void;
declare function subscribe(request: any): Promise<any>;
declare function validateEmail(emailAddress: string): boolean;
