export interface Todo{
    id:number
    title:string
    description:string
    isActive:boolean
    createDate:Date
}

export interface CreateTodoRequest{
    title:string
    description:string
    isActive:boolean
    createDate:Date
}

export interface UpdateTodoRequest{
    title:string
    description:string
    isActive:boolean
    createDate:Date
    updateDate:Date
}